
$(document).ready(() => {
    $('#btnExcel').on('click', EnviarExcel)
    cargarExcel()
})

var cargarExcel = () => {
    let url = `${baseUrl}Excel/ListarExcels/1`;
    fetch(url).then(r => r.json()).then(m => {
        j = m.list
        let contenido = ``;
        if (j.length > 0) {
            contenido = j.map((x, y) => {
                let num = `<td>${x.ID_EXCEL}</td>`
                let nombre = `<td>${x.NOMBRE}</td>`
                let anio = `<td>${x.ANIO}</td>`
                let mes = `<td>${x.NOMBRE_MES}</td>`
                return `<tr>${num}${nombre}${anio}${mes}</tr>`;
            }).join('');;
        }
        $('#tbl-excel').find('tbody').html(contenido)
    });
}

var EnviarExcel = () => {
    if (!validarInput()) return
    $("#form-excel").submit();
}

var validarInput = () => {
    $('.alert-add').html('');
    let arr = [];
    if ($('#excel').val() == "") arr.push("Debe seleccionar un archivo para cargar");
    if (!($('#excel').val().endsWith(".xlsx") || $('#excel').val().endsWith(".xlsm") || $('#excel').val().endsWith(".xls"))) arr.push("Debe seleccionar un archivo excel válido (xlsx, xlsm, xls)");
    if ($('#anio').val() == 0) arr.push("Debe seleccionar un año");
    if ($('#mes').val() == 0) arr.push("Debe seleccionar un mes");

    if (arr.length > 0) {
        let error = '';
        $.each(arr, function (ind, elem) { error += '<li><small class="mb-0">' + elem + '</li></small>'; });
        error = `<ul>${error}</ul>`;
        $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: error });
        return false
    }
    return true
}

$("#form-excel").submit(function () {
    var form = $(this);

    if (form.validate()) {        
        form.ajaxSubmit({
            dataType: 'JSON',
            type: 'POST',
            url: form.attr('action'),
            success: function (r) {
                if (r.success) {
                    $('#excel').val('');
                    $('#anio').val(0);
                    $('#mes').val(0)
                }
                alert(r.mensaje)
            },
            error: function (jqXHR, textStatus, errorThrown) {
                //alert(errorThrown);
                console.log('Hubo un problema con la petición Fetch:' + errorThrown.message);
                location.href = `${baseUrl}Inicio/Index`
            },
            beforeSend: function () {
                $('#excel').prop('disabled', true)
                $('#anio').prop('disabled', true)
                $('#mes').prop('disabled', true)
                $('#btnExcel').prop('disabled', true)
                $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i> Cargando...");
            },
            complete: function () {
                cargarExcel()
                $('#excel').prop('disabled', false)
                $('#anio').prop('disabled', false)
                $('#mes').prop('disabled', false)
                $('#btnExcel').prop('disabled', false)
                $("#preload").html("");
            }
        });
    }

    //let url = `${baseUrl}Inicio/GuardarBD`;
    //let init = { method: 'POST', headers: { 'Content-Type': 'application/json'} };

    //fetch(url, init)
    //.then(r =>  r.json())
    //.then(j => {
    //    debugger
    //    alert("entre")
    //    console.log(j)
    //});
    return false;
})
