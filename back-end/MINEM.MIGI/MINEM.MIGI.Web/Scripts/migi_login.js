
$(document).ready(() => {
    $('#btnIniciar').on('click', onSubmit)
})

function onSubmit(token) {
    $("#form-login").submit();
}

$("#form-login").submit(function () {
    var form = $(this);

    if (form.validate()) {
        form.ajaxSubmit({
            dataType: 'JSON',
            type: 'POST',
            url: form.attr('action'),
            success: function (r) {
                debugger;
                if (r.success)
                    location.href = `${baseUrl}${r.message}`
                else
                    if (r.tipo == "Error")
                        alert(r.message)
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
        });
    }
    return false;
})