// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    if ('Notification' in window && 'serviceWorker' in navigator) {
        Notification.requestPermission().then(function (permission) {
            if (permission === 'granted') {
                navigator.serviceWorker.ready.then(function (registration) {
                    registration.pushManager.subscribe({
                        userVisibleOnly: true,
                        applicationServerKey: urlBase64ToUint8Array('SUA_CHAVE_PUBLICA')
                    }).then(function (subscription) {
                        // Enviar a inscrição para o backend
                        fetch('/api/pushNotification/subscribe', {
                            method: 'POST',
                            body: JSON.stringify(subscription),
                            headers: {
                                'Content-Type': 'application/json'
                            }
                        });
                    });
                });
            }
        });
    }
});

function urlBase64ToUint8Array(base64String) {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');
    var rawData = window.atob(base64);
    var outputArray = new Uint8Array(rawData.length);
    for (var i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
