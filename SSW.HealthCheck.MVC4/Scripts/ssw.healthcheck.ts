module ssw {
    export module healthcheck {
        export interface ITestMonitor {
            Key: string;
            Name: string;
            Description: string;
            IsRunning: boolean;
            Result: {
                Success: boolean;
                Message: string;
            }
            Events: ITestEvent[];
            Progress: IProgress;
        }
        export interface ITestChanged {
            Key: string;
            IsRunning: boolean;
            Result: {
                Success: boolean;
                Message: string;
            }
        }
        export interface ITestEvent {
            DateTime: Date;
            Message: string;
        }
        export interface IProgress {
            Min: number;
            Max: number;
            Val: number
        }

        export class HealthCheckController {
            $http: any;
            UpdateStats: (data) => void;
            Check: (model: ITestMonitor, reset: boolean) => void;
            CheckAll: () => void;
            CheckAllDefault: () => void;
            OnTestStarted: (x: ITestChanged) => void;
            OnTestCompleted: (x: ITestChanged) => void;
            OnTestEvent: (x: { Key: string; Event: ITestEvent }) => void;
            OnTestProgress: (x: { Key: string; Progress: IProgress }) => void;

            constructor($scope: any, $http: any, tests: ITestMonitor[]) {
                $scope.tests = tests;

                // convert tests into mapping
                var testsByKey = {};
                var allTests = [];
                var failed = [];
                var warning = [];
                var passed = [];

                for (var i = 0; i < $scope.tests.length; i++) {
                    for (var j = 0; j < $scope.tests[i].TestMonitors.length; j++) {
                        allTests.push($scope.tests[i].TestMonitors[j]);
                    }
                }

                for (var k in allTests) {
                    var t = allTests[k];
                    testsByKey[t.Key] = t;
                }

                this.$http = $http;
                this.UpdateStats = (data) => {
                    var all = allTests.length;
                    if (data.Result.Success && !data.Result.ShowWarning) {
                        passed.push(data.Key);
                    }

                    if (data.Result.Success && data.Result.ShowWarning) {
                        warning.push(data.Key);
                    }

                    if (!data.Result.Success) {
                        failed.push(data.Key);
                    }

                    $("#all-stat").text(all);
                    $("#passed-stat").text(passed.length);
                    $("#warning-stat").text(warning.length);
                    $("#failed-stat").text(failed.length);
                    $("#pending-stat").text((all - passed.length - warning.length - failed.length));
                };
                this.Check = (model: ITestMonitor, reset) => {
                    var that = this;
                    if (!reset) {
                        var index = passed.indexOf(model.Key);
                        if (index > -1) {
                            passed.splice(index, 1);
                        } else {
                            index = failed.indexOf(model.Key);
                            if (index > -1) {
                                failed.splice(index, 1);
                            } else {
                                index = warning.indexOf(model.Key);
                                if (index > -1) {
                                    warning.splice(index, 1);
                                }
                            }
                        }
                    }

                    $http.get(($("#ng-app").data("root-path") || "/") + "HealthCheck/Check?Key=" + model.Key)
                        .success((data: any, status: any, headers: any, config: any) => {
                            that.UpdateStats(data);
                        })
                        .error((data: any, status: any, headers: any, config: any) => {
                            console.log(data);
                        });
                };
                this.CheckAll = () => {
                    passed.length = 0;
                    failed.length = 0;
                    warning.length = 0;
                    for (var k in allTests) {
                        var test = allTests[k];
                        this.Check(test, true);
                    }
                };
                this.CheckAllDefault = () => {
                    passed.length = 0;
                    failed.length = 0;
                    warning.length = 0;
                    for (var k in allTests) {
                        var test = allTests[k];
                        if (!test.IsRunning && test.IsDefault && !test.Result) {
                            // do not automatically re-run tests that has failed.
                            // this enable better troubleshooting of errors reported by the stats api
                            this.Check(test, true);
                        }
                    }
                };
                this.OnTestStarted = (x: ITestChanged) => {
                    var test: ITestMonitor = testsByKey[x.Key];
                    if (test) {
                        test.IsRunning = x.IsRunning;
                        test.Result = x.Result;
                        test.Events = [];
                        $scope.$apply();
                    }
                };
                this.OnTestCompleted = (x: ITestChanged) => {
                    var test: ITestMonitor = testsByKey[x.Key];
                    if (test) {
                        test.IsRunning = x.IsRunning;
                        test.Result = x.Result;
                        $scope.$apply();
                    }
                };
                this.OnTestEvent = (x: { Key: string; Event: ITestEvent }) => {
                    console.log(x);
                    var test: ITestMonitor = testsByKey[x.Key];
                    if (test) {
                        test.Events.push(x.Event);
                        $scope.$apply();
                    }
                };

                this.OnTestProgress = (x: { Key: string; Progress: IProgress }) => {
                    console.log(x);
                    var test: ITestMonitor = testsByKey[x.Key];
                    if (test) {
                        test.Progress = x.Progress;
                        $scope.$apply();
                    }
                };

                // signalr
                $.connection.hub.logging = true;
                var healthCheckHub = (<any>$.connection).healthCheckHub;
                var healthCheckServer = healthCheckHub.server;
                var healthCheckClient = healthCheckHub.client;
                healthCheckClient.testCompleted = this.OnTestCompleted;
                healthCheckClient.testStarted = this.OnTestStarted;
                healthCheckClient.testEvent = this.OnTestEvent;
                healthCheckClient.testProgress = this.OnTestProgress;
                $.connection.hub.start().done(() => {
                    this.CheckAllDefault();
                });
            }
        }
    }

    var hcheck = angular.module('ssw.healthcheck', <string[]>[]);
    hcheck.filter('toTrusted', function ($sce) {
        return function (val) {
            return $sce.trustAsHtml(val);
        };
    });
    hcheck.value('tests', <string[]>[]);
    hcheck.controller('HealthCheck', ['$scope', '$http', 'tests', ssw.healthcheck.HealthCheckController]);
}