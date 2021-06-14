
$(document).ready(() => {
    $('#btnIniciar').on('click', onSubmit)
})

function onSubmit(token) {
    $("#frmLogin").submit();
}

$("#frmLogin").submit(function () {
    var form = $(this);

    if (form.validate()) {
        form.ajaxSubmit({
            dataType: 'JSON',
            type: 'POST',
            url: form.attr('action'),
            success: function (r) {
                if (r.success)
                    location.href = `${baseUrl}${r.message}`
                else
                    if (r.tipo == "Error") {
                        $('#seccion-msj').html(mensajeError('Error de acceso', r.message, 1))
                        setTimeout(() => {
                            $('#seccion-msj').html('')
                        }, 4000)
                    }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
        });
    }
    return false;
})

