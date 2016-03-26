angular.module('sarAuth')
.controller('RegisterLoginCtrl', ['$scope', '$http', '$window', function ($scope, $http, $window) {
  angular.extend($scope, {
    init: function (email) {
      $scope.model = { email: email };
    },

    sending: false,
    codeSent: false,
    verifying: false,

    sendCode: function () {
      if ($scope.emailForm.$invalid) {
        return;
      }

      $scope.sending = true;
      $scope.codeSent = false;

      $http({
        method: 'POST',
        url: window.appRoot + 'externalVerificationCode',
        data: { email: $scope.model.email }
      }).then(function (resp) {
        if (resp.data.success == true) { $scope.codeSent = true; }
      }, function (resp) {
        alert('Error: ' + resp.statusText);
      })['finally'](function () {
        $scope.sending = false;
      })
    },
    verify: function () {
      $scope.verifying = true;
      $http({
        method: 'POST',
        url: window.appRoot + 'verifyExternalCode',
        data: { code: $scope.model.code, email: $scope.model.email }
      }).then(function (resp) {
        if (resp.data.success)
          $window.location.href = resp.data.url;
      }, function (resp) {
        alert('Error: ' + resp.statusText);
      })['finally'](function () {
        $scope.verifying = false;
      })
    }
  })
}]);