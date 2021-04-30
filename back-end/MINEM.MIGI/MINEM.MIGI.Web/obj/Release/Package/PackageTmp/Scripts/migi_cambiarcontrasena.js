$(document).ready(() => {
    $('form').submit((e) => {
        sendLogin(e);
    })
    $('#txt-pswd').on('blur', (e) => seguridadinicial());
});

var sendLogin = (e) => {
    e.preventDefault();
    $('.alert-add').html('');
    let arr = [];

    if ($("#txt-pswd").val().trim() === $("#txt-con").val().trim()) {
        if ($("#txt-pswd").val().trim().length < 6) arr.push("La contraseña debe contener 6 o más caracteres por seguridad");
    }
    else arr.push("Las contraseñas no coinciden");

    if (arr.length > 0) {
        let error = '';
        $.each(arr, function (ind, elem) { error += '<li><small class="mb-0">' + elem + '</small></li>'; });
        error = `<ul>${error}</ul>`;
        $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: error });
        return;
    }

    let contrasena = $('#txt-pswd').val().trim();

    let url = `${baseUrl}Inicio/NuevaContrasena`; //end point 35
    let data = { ID_USUARIO: idusuario, CONTRASENA_NUEVO: contrasena };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };

    fetch(url, init)
    .then(r => r.json())
    .then(m => {
        j = m.Estado
        let mensaje = '';
        mensaje = j == 0 ? 'Hubo un problema, inténtelo nuevamente por favor.' : j == 1 ? 'Hubo un problema, inténtelo nuevamente por favor.' : j == 2 ? 'La contraseña se cambio correctamente.' : '';
        $('.alert-add').html('');
        if (j == 2) { $('#btn-cambiar').hide(); }
        if (j == 2) $('.alert-add').alertSuccess({ type: 'success', title: 'BIEN HECHO', message: mensaje, close: { time: 4000 }, url: `${baseUrl}Inicio` });
        else $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: mensaje });
    });
}

$(document).on("keydown", ".sin-espacio", function (e) {
    var key = window.e ? e.which : e.keyCode;
    if (key == 32) return false;
});

var seguridadinicial = () => {
    if ($('#txt-pswd').val().trim() == "") {
        $('.spanNivelesColores').removeClass().addClass('spanNivelesColores');
        $('#nivelseguridad > span').html('');
    }
}

