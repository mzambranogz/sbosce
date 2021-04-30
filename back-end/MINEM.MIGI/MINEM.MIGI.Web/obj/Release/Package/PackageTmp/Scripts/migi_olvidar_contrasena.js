
var enviarMail = (e) => {
    e.preventDefault();
    let correo = $('#txt-user').val();

    let url = `${baseUrl}Inicio/RecuperarContrasena?correo=${correo}`;

    fetch(url)
    .then(r => r.json())
    .then(responseEnvioMail);
};

var responseEnvioMail = (data) => {
    let success = data.success;
    let message = data.message;

    if (success == true) {
        $('#alerta').alertSuccess({ type: 'success', title: 'Validando credenciales', message });
        setTimeout(redirigir, 5000);
    }
    else {
        $('#alerta').alertError({ type: 'danger', title: 'Validando credenciales', message });
        setTimeout(limpiarAlert, 2500);
    }
};

var redirigir = () => {
    location.href = `${baseUrl}Inicio`;
}

var limpiarAlert = () => {
    $('#alerta').html();
}

$(document).ready(() => {
    $('#frmLogin').submit(enviarMail);
});