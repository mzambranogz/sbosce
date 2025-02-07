﻿
$(document).ready(() => {
    $('.b-activo').removeClass('nav-active')
    $('.v-cargamasiva').addClass('nav-active')
    $('#btnExcel').on('click', EnviarExcel)
    $('#btnConfirmar').on('click', (e) => eliminar());
    cargarExcel()
})

var fn_avance_grilla = (boton) => {
    var total = 0, miPag = 0;
    miPag = Number($("#ir-pagina").val());
    total = Number($(".total-paginas").html());

    if (boton == 1) miPag = 1;
    if (boton == 2) if (miPag > 1) miPag--;
    if (boton == 3) if (miPag < total) miPag++;
    if (boton == 4) miPag = total;

    $("#ir-pagina").val(miPag);
    cargarExcel();
}

var cambiarPagina = () => {
    cargarExcel();
}

$(".columna-filtro").click(function (e) {
    var id = e.target.id;

    $(".columna-filtro").removeClass("fa-sort-up");
    $(".columna-filtro").removeClass("fa-sort-down");
    $(".columna-filtro").addClass("fa-sort");

    if ($("#columna").val() == id) {
        if ($("#orden").val() == "ASC") {
            $("#orden").val("DESC")
            $(`#${id}`).removeClass("fa-sort");
            $(`#${id}`).removeClass("fa-sort-up");
            $(`#${id}`).addClass("fa-sort-down");
        }
        else {
            $("#orden").val("ASC")
            $(`#${id}`).removeClass("fa-sort");
            $(`#${id}`).removeClass("fa-sort-down");
            $(`#${id}`).addClass("fa-sort-up");
        }
    }
    else {
        $("#columna").val(id);
        $("#orden").val("ASC")
        $(`#${id}`).removeClass("fa-sort");
        $(`#${id}`).removeClass("fa-sort");
        $(`#${id}`).addClass("fa-sort-up");
    }

    cargarExcel();
});

var cargarExcel = () => {
    let tipoexcel = 1
    let registros = $('#catidad-rgistros').val();
    let pagina = $('#ir-pagina').val();
    let columna = $("#columna").val();
    let orden = $("#orden").val();
    let params = {tipoexcel, registros, pagina, columna, orden };
    let queryParams = Object.keys(params).map(x => params[x] == null ? x : `${x}=${params[x]}`).join('&');

    /*let url = `${baseUrl}Excel/ListarExcels/1`;
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
    });*/

    let url = `${baseUrl}Excel/ListarExcels?${queryParams}`;
    fetch(url).then(r => r.json()).then(m => {
        if (m == null) return
        j = m.list
        let contenido = ``, cont = 0;
        let tabla = $('#tbl');
        tabla.find('tbody').html('');
        $('#viewPagination').attr('style', 'display: none !important');
        if (j.length > 0) {
            if (j[0].CANTIDAD_REGISTROS == 0) { $('#viewPagination').hide(); $('#view-page-result').hide(); }
            else { $('#view-page-result').show(); $('#viewPagination').show(); }
            $('.inicio-registros').text(j[0].CANTIDAD_REGISTROS == 0 ? 'No se encontraron resultados' : (j[0].PAGINA - 1) * j[0].CANTIDAD_REGISTROS + 1);
            $('.fin-registros').text(j[0].TOTAL_REGISTROS < j[0].PAGINA * j[0].CANTIDAD_REGISTROS ? j[0].TOTAL_REGISTROS : j[0].PAGINA * j[0].CANTIDAD_REGISTROS);
            $('.total-registros').text(j[0].TOTAL_REGISTROS);
            $('.pagina').text(j[0].PAGINA);
            $('#ir-pagina').val(j[0].PAGINA);
            $('#ir-pagina').attr('max', j[0].TOTAL_PAGINAS);
            $('.total-paginas').text(j[0].TOTAL_PAGINAS);

            contenido = j.map((x, y) => {
                let formatoCodigo = '000000';
                let colNro = `<td class="text-center" data-encabezado="Número de orden" scope="row" data-count="0">${(pagina - 1) * registros + (y + 1)}</td>`;
                let colCodigo = `<td class="text-center" data-encabezado="Código" scope="row"><span>${(`${formatoCodigo}${x.ID_EXCEL}`).split('').reverse().join('').substring(0, formatoCodigo.length).split('').reverse().join('')}</span></td>`
                let colNombres = `<td class="text-left" data-encabezado="Nombre excel">Resultado N°${x.NOMBRE}</td>`;
                let colAnio = `<td class="text-center" data-encabezado="Año">${x.ANIO}</td>`;
                let colMes = `<td class="text-center" data-encabezado="Mes">${x.NOMBRE_MES}</td>`;
                //let btnCambiarEstado = `<a class="dropdown-item estilo-01 btnCambiarEstado" href="javascript:void(0)" data-id="${x.ID_RESULTADO}" data-estado="${x.FLAG_ESTADO}"><i class="fas fa-edit mr-1"></i>Eliminar</a>`;
                //let btnEditar = `<a class="dropdown-item estilo-01 btnEditar" href="${baseUrl}Resultado/VerResultado/${x.ID_RESULTADO}" data-id="${x.ID_RESULTADO}"><i class="fas fa-edit mr-1"></i>Ver resultado</a>`;

                let btnCambiarEstado = `<a class="dropdown-item estilo-01 btnCambiarEstado" href="javascript:void(0)" data-id="${x.ID_EXCEL}" data-estado="${x.FLAG_ESTADO}"><i class="fas fa-edit mr-1"></i>Eliminar</a>`;
                let colOpciones = `<td class="text-center" data-encabezado="Gestión"><div class="btn-group w-100"><a class="btn btn-sm bg-success text-white w-100 dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" tabindex="0">Gestionar</a><div class="dropdown-menu">${btnCambiarEstado}</div></div></td>`;
                let fila = `<tr>${colNro}${colCodigo}${colNombres}${colAnio}${colMes}${colOpciones}</tr>`;
                return fila;
            }).join('');

            tabla.find('tbody').html(contenido)

            tabla.find('.btnCambiarEstado').each(x => {
                let elementButton = tabla.find('.btnCambiarEstado')[x];
                $(elementButton).on('click', (e) => {
                    e.preventDefault();
                    cambiarEstado(e.currentTarget);
                });
            });
        } else {
            $('#viewPagination').hide(); $('#view-page-result').hide();
            $('.inicio-registros').text('No se encontraron resultados');
        }        
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
        $('#txt-excel').val('')
        $('#excel').val('')
        return false
    }
    return true
}

var archivoexcel
$("#form-excel").submit(function () {    
    var form = $(this);

    //$('#excel').prop('disabled', true)
    //$('#anio').prop('disabled', true)
    //$('#mes').prop('disabled', true)
    //$('#btnExcel').prop('disabled', true)
    //let num = $('#inicio').val() == 0 ? 0 : 250000 / Parse.Int($('#inicio').val())
    //$("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i>" + Math.round(num) + "% Cargando...");

    if (form.validate()) {        
        form.ajaxSubmit({
            dataType: 'JSON',
            type: 'POST',
            url: form.attr('action'),
            success: function (r) {
                if (r.success) {
                    if (r.continuar) {
                        $('#inicio').val(r.inicio)
                        $('#fin').val(r.fin)
                        $('#idExcel').val(r.idexcel)
                        $("#form-excel").submit();
                    } else {
                        $('#excel').val('');
                        $('#anio').val(0);
                        $('#mes').val(0)
                        $('.alert-add').alertSuccess({ type: 'success', title: 'Bien hecho', message: r.mensaje });
                        $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i>" + 100 + "% Cargando...");
                        setTimeout(() => {
                            $('.alert-add').html('')
                            $("#preload").html("");
                        }, 4000)
                        $('#txt-excel').val('')
                        $('#excel').val('')

                        cargarExcel()
                        $('#btnExcel').prop('disabled', false)                        
                    }                    
                } else {
                    $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: r.mensaje });
                    $('#btnExcel').prop('disabled', false)
                    $("#preload").html("");
                }
                //$('#txt-excel').val('')
                //$('#excel').val('')
            },
            error: function (jqXHR, textStatus, errorThrown) {
                //alert(errorThrown);
                console.log('Hubo un problema con la petición Fetch:' + errorThrown.message);
                //location.href = `${baseUrl}Inicio/Index`
            },
            beforeSend: function () {
                //$('#excel').prop('disabled', true)
                //$('#anio').prop('disabled', true)
                //$('#mes').prop('disabled', true)
                //$('#btnExcel').prop('disabled', true)
                let tamanio = $('#excel')[0].files[0].size
                // Peso -> 27148000 / cantidad reg -> 137484 peso por reg -> 202.20 | Peso Archivo / Cant reg = peso por reg
                let totalreg = tamanio / 202.20
                let num = $('#inicio').val() == 0 ? 0 : 100 / (Math.round(totalreg) / parseInt($('#inicio').val()))
                $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i>" + Math.round(num > 100 ? 100 : num) + "% Cargando...");
            },
            complete: function () {
                //cargarExcel()
                //$('#excel').prop('disabled', false)
                //$('#anio').prop('disabled', false)
                //$('#mes').prop('disabled', false)
                //$('#btnExcel').prop('disabled', false)
                //$("#preload").html("");
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

var cambiarEstado = (element) => {
    idEliminar = $(element).attr('data-id');
    $("#modal-confirmacion").modal('show');
};

var eliminar = () => {
    if (idEliminar == 0) return;
    let data = { ID_EXCEL: idEliminar, ID_TIPO_EXCEL: 1 };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };
    let url = `${baseUrl}Excel/EliminarExcel`;
    fetch(url, init)
        .then(r => r.json())
        .then(j => {
            if (j.success) {
                cargarExcel();
                $("#modal-confirmacion").modal('hide');
                $('.alert-add').alertSuccess({ type: 'success', title: 'Bien hecho', message: 'Se eliminó correctamente' });
                setTimeout(() => {
                    $('.alert-add').html('')
                }, 4000)
            } else 
                $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: 'Ocurrió un problema al eliminar el archivo escel y sus registros' });
        });
}
