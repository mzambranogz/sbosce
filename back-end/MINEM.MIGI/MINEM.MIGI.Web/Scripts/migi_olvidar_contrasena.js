
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
        $('#seccion-msj').html(mensajeSuccess(message))
        setTimeout(redirigir, 5000);
    }
    else {
        $('#seccion-msj').html(mensajeError('Error de recuperación', message, 1))
        setTimeout(() => {
            $('#seccion-msj').html('')
        }, 3500);
    }
};

var redirigir = () => {
    location.href = `${baseUrl}Inicio`;
}

$(document).ready(() => {
    $('#frmLogin').submit(enviarMail);
});