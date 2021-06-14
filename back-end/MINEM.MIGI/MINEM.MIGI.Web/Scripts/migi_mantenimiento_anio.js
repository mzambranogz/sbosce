$(document).ready(() => {
    //$('#btnConsultar').on('click', (e) => consultar());
    //$('#btnConsultar')[0].click();
    //$('#btnNuevo').on('click', (e) => nuevo());
    //$('#btnCerrar').on('click', (e) => cerrarFormulario());
    //$('#btnGuardar').on('click', (e) => guardar());
    $('.b-activo').removeClass('nav-active')
    $('.v-mantenimiento').addClass('nav-active')
    $('#ir-pagina').on('change', (e) => cambiarPagina());
    $('#catidad-rgistros').on('change', (e) => cambiarPagina());
    $('#btnConsultar').on('click', (e) => consultar());
    $('#btnConsultar')[0].click();
    $('#btnNuevo').on('click', (e) => nuevo());
    $('#btnConfirmar').on('click', (e) => eliminar());
    $('#btnGuardar').on('click', (e) => guardar());
});

var fn_avance_grilla = (boton) => {
    var total = 0, miPag = 0;
    miPag = Number($("#ir-pagina").val());
    total = Number($(".total-paginas").html());

    if (boton == 1) miPag = 1;
    if (boton == 2) if (miPag > 1) miPag--;
    if (boton == 3) if (miPag < total) miPag++;
    if (boton == 4) miPag = total;

    $("#ir-pagina").val(miPag);
    $('#btnConsultar')[0].click();
}

var cambiarPagina = () => {
    $('#btnConsultar')[0].click();
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

    $('#btnConsultar')[0].click();
});

var consultar = () => {
    let busqueda = $('#txt-descripcion').val();;
    let registros = $('#catidad-rgistros').val();
    let pagina = $('#ir-pagina').val();
    let columna = $("#columna").val();
    let orden = $("#orden").val();
    let params = { busqueda, registros, pagina, columna, orden };
    let queryParams = Object.keys(params).map(x => params[x] == null ? x : `${x}=${params[x]}`).join('&');

    let url = `${baseUrl}Mantenimiento/BuscarAnios?${queryParams}`;

    fetch(url).then(r => r.json()).then(m => {
        let tabla = $('#tbl');
        tabla.find('tbody').html('');
        $('#viewPagination').attr('style', 'display: none !important');
        j = m.list
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

            let cantidadCeldasCabecera = tabla.find('thead tr th').length;
            let contenido = renderizar(j, cantidadCeldasCabecera, pagina, registros);
            tabla.find('tbody').html(contenido);
            tabla.find('.btnCambiarEstado').each(x => {
                let elementButton = tabla.find('.btnCambiarEstado')[x];
                $(elementButton).on('click', (e) => {
                    e.preventDefault();
                    cambiarEstado(e.currentTarget);
                });
            });
            tabla.find('.btnEditar').each(x => {
                let elementButton = tabla.find('.btnEditar')[x];
                $(elementButton).on('click', (e) => {
                    e.preventDefault();
                    consultarEntidad(e.currentTarget);
                });
            });
        } else {
            $('#viewPagination').hide(); $('#view-page-result').hide();
            $('.inicio-registros').text('No se encontraron resultados');
        }
    });
};

var renderizar = (data, cantidadCeldas, pagina, registros) => {
    let deboRenderizar = data.length > 0;
    let contenido = `<tr><th colspan='${cantidadCeldas}'>No existe información</th></tr>`;

    if (deboRenderizar) {
        contenido = data.map((x, i) => {
            let formatoCodigo = '00000000';
            let colNro = `<td class="text-center" data-encabezado="Número de orden" scope="row" data-count="0">${(pagina - 1) * registros + (i + 1)}</td>`;
            let colCodigo = `<td class="text-center" data-encabezado="Código" scope="row"><span>${(`${formatoCodigo}${x.ID_ANIO}`).split('').reverse().join('').substring(0, formatoCodigo.length).split('').reverse().join('')}</span></td>`;
            let colNombres = `<td class="text-center" data-encabezado="Año">${x.ANIO}</td>`;
            let btnEditar = `<a class="dropdown-item btnEditar" href="javascript:void(0)" data-id="${x.ID_ANIO}" data-toggle="modal" data-target="#modal-mantenimiento"><i class="fas fa-edit mr-1"></i>Editar</a>`;
            //let btnCambiarEstado = `${[0, 1].includes(x.FLAG_ESTADO) ? "" : `<a href="javascript:void(0)" data-id="${x.ID_ANIO}" data-estado="${x.FLAG_ESTADO}" class="dropdown-item btnCambiarEstado"><i class="fas fa-eraser mr-1"></i>Eliminar</a> `}`;
            let btnCambiarEstado = `${[0, 1].includes(x.FLAG_ESTADO) ? "" : `<a class="dropdown-item btnCambiarEstado" href="#" data-id="${x.ID_ANIO}" data-estado="${x.FLAG_ESTADO}"><i class="fas fa-eraser mr-1"></i>Eliminar</a>`}`;
            let colOpciones = `<td class="text-center text-xs-right" data-encabezado="Acciones"><div class="btn-group"><div class="acciones fase-01 dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fas fa-ellipsis-h"></i></div><div class="dropdown-menu dropdown-menu-right">${btnEditar}${btnCambiarEstado}</div></div></div></td>`;
            let fila = `<tr>${colNro}${colCodigo}${colNombres}${colOpciones}</tr>`;
            return fila;
        }).join('');
    };

    return contenido;
};

var cambiarEstado = (element) => {
    idEliminar = $(element).attr('data-id');
    $("#modal-confirmacion").modal('show');    
};

var eliminar = () => {
    if (idEliminar == 0) return;
    let data = { ID_ANIO: idEliminar, UPD_USUARIO: idUsuarioLogin };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };
    let url = `${baseUrl}Mantenimiento/EliminarAnio`;

    fetch(url, init)
    .then(r => r.json())
    .then(j => {
        if (j.success) {
            $("#modal-confirmacion").modal('hide');
            $('#btnConsultar')[0].click();
        } else {
            alert('Ocurrió un problema al momento de eliminar el registro')
        }
    })
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
}

var nuevo = () => {
    $('#frm').show();
    limpiarFormulario();
}

var nuevo = () => {
    $('#frm').removeData('id');
    $('.alert-add').html('');
    $('#btnGuardar').show();
    $('#btnGuardar').next().html('Cancelar');
    $('#exampleModalLabel').html('Registro de nuevo año <br><small class="text-muted">Complete los siguientes campos para registrar un nuevo año</small><small class="text-danger d-block"><strong>(*)&nbsp;</strong>Campos obligatorios</small>');
    limpiarDatos();
}

var limpiarDatos = () => {
    $('#txtAnno').val('');
}

//var cerrarFormulario = () => {
//    $('#frm').hide();
//}

//var limpiarFormulario = () => {
//    $('#frm').removeData();
//    $('#txtAnno').val('');
//}

var consultarEntidad = (element) => {
    $('.alert-add').html('');
    $('#btnGuardar').show();
    $('#btnGuardar').next().html('Cancelar');
    $('#exampleModalLabel').html('Actualización de año <br><small class="text-muted">Puede cambiar los datos mostrados para actualizar un año</small><small class="text-danger d-block"><strong>(*)&nbsp;</strong>Campos obligatorios</small>');
    limpiarDatos();
    let id = $(element).attr('data-id');

    let url = `${baseUrl}Mantenimiento/ObtenerAnio?idanio=${id}`;

    fetch(url)
    .then(r => r.json())
    .then(j => {
        cargarDatos(j.obj);
    })
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
}

var cargarDatos = (data) => {
    if (data == null) return
    $('#frm').data('id', data.ID_ANIO);
    $('#txtAnno').val(data.ANIO);
}

var guardar = () => {
    let arr = [];
    let id = $('#frm').data('id');
    let nombre = $('#txtAnno').val();

    if (nombre.trim() == "") arr.push('Debe ingresar un año');

    if (arr.length > 0) {
        let error = '';
        $.each(arr, function (ind, elem) { error += '<li><small class="mb-0">' + elem + '</li></small>'; });
        error = `<ul>${error}</ul>`;
        $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: error });
        return;
    }

    let url = `${baseUrl}Mantenimiento/GuardarAnio`;
    let data = { ID_ANIO: id == null ? -1 : id, ANIO: nombre, UPD_USUARIO: idUsuarioLogin };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };

    fetch(url, init)
    .then(r => r.json())
    .then(j => {
        if (j.success) {
            $('.alert-add').html('');
            if (j.success) { $('#btnGuardar').hide(); $('#btnGuardar').next().html('Cerrar'); }
            j.success ? $('.alert-add').alertSuccess({ type: 'success', title: 'BIEN HECHO', message: 'Los datos fueron guardados correctamente.', close: { time: 1000 }, url: `` }) : $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: 'Inténtelo nuevamente por favor.' });
            if (j.success) $('#btnConsultar')[0].click();
        }
    })
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
}