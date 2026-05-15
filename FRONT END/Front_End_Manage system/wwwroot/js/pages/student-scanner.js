// student-scanner.js - QR Code Scanner for Attendance

let video = null;
let canvas = null;
let context = null;
let stream = null;
let scanning = false;
let animationId = null;

document.addEventListener('DOMContentLoaded', function () {
    video = document.getElementById('video');
    canvas = document.getElementById('canvas');
    if (canvas) context = canvas.getContext('2d');

    const startBtn = document.getElementById('startCameraBtn');
    const stopBtn = document.getElementById('stopCameraBtn');

    if (startBtn) startBtn.addEventListener('click', startCamera);
    if (stopBtn) stopBtn.addEventListener('click', stopCamera);
});

async function startCamera() {
    try {
        stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: 'environment' }
        });
        video.srcObject = stream;
        await video.play();

        document.getElementById('startCameraBtn').style.display = 'none';
        document.getElementById('stopCameraBtn').style.display = 'inline-block';

        startScanning();
    } catch (err) {
        console.error('Camera error:', err);
        showResult('Camera access denied or not available. Please check your permissions.', 'error');
    }
}

function stopCamera() {
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        video.srcObject = null;
    }
    scanning = false;
    if (animationId) {
        cancelAnimationFrame(animationId);
        animationId = null;
    }

    document.getElementById('startCameraBtn').style.display = 'inline-block';
    document.getElementById('stopCameraBtn').style.display = 'none';
}

function startScanning() {
    scanning = true;
    scanQRCode();
}

async function scanQRCode() {
    if (!scanning) return;

    if (video.readyState === video.HAVE_ENOUGH_DATA) {
        if (video.videoWidth > 0 && video.videoHeight > 0) {
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            context.drawImage(video, 0, 0, canvas.width, canvas.height);

            const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
            const code = jsQR(imageData.data, canvas.width, canvas.height);

            if (code) {
                scanning = false;
                stopCamera();
                await processQRCode(code.data);
                return;
            }
        }
    }

    animationId = requestAnimationFrame(scanQRCode);
}

async function processQRCode(qrData) {
    showResult('Processing QR code...', 'info');

    const token = getAntiForgeryToken();
    const formData = new URLSearchParams();
    formData.append('__RequestVerificationToken', token);
    formData.append('qrData', qrData);

    try {
        const response = await fetch('/Student/ProcessScan', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            showResult('✓ ' + result.message, 'success');
            setTimeout(function () {
                window.location.href = '/Student/Dashboard';
            }, 2000);
        } else {
            showResult('✗ ' + result.message, 'error');
            setTimeout(function () {
                startCamera();
            }, 3000);
        }
    } catch (error) {
        console.error('Scan error:', error);
        showResult('Connection error. Please try again.', 'error');
        setTimeout(function () {
            startCamera();
        }, 3000);
    }
}

function showResult(message, type) {
    const resultDiv = document.getElementById('scanResult');
    if (!resultDiv) return;

    resultDiv.textContent = message;
    resultDiv.className = 'scan-result ' + type;
}

function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}