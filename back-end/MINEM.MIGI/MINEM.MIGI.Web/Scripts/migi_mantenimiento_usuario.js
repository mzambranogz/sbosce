
$(document).ready(() => {
    $('.b-activo').removeClass('nav-active')
    $('.v-mantenimiento').addClass('nav-active')
    $('#ir-pagina').on('change', (e) => cambiarPagina());
    $('#catidad-rgistros').on('change', (e) => cambiarPagina());
    $('#btnConsultar').on('click', (e) => consultar());
    $('#btnConsultar')[0].click();
    $('#btnNuevo').on('click', (e) => nuevoUsuario());
    $('#btnGuardar').on('click', (e) => verificar());
});
var flag_ndc = '0';
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

    let url = `${baseUrl}Mantenimiento/BuscarUsuarios?${queryParams}`;

    fetch(url).then(r => r.json()).then(m => {
        let tabla = $('#tblUsuario');
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

            tabla.find('.btnEditar').each(x => {
                let elementButton = tabla.find('.btnEditar')[x];
                $(elementButton).on('click', (e) => {
                    e.preventDefault();
                    consultarUsuario(e.currentTarget);
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
            let colNro = `<th class="text-center" data-encabezado="Número" scope="row">${(pagina - 1) * registros + (i + 1)}</th>`;
            let colCodigo = `<td data-encabezado="Código">${(`${formatoCodigo}${x.ID_USUARIO}`).split('').reverse().join('').substring(0, formatoCodigo.length).split('').reverse().join('')}</td>`;
            let colNombres = `<td data-encabezado="Nombres">${x.NOMBRES}</td>`;
            let colApellidos = `<td data-encabezado="Apellidos">${x.APELLIDOS}</td>`;
            let colCorreo = `<td data-encabezado="Correo">${x.CORREO}</td>`;
            let colEstado = `<td data-encabezado="Estado" class="text-center"><div class="badge badge-${x.FLAG_ESTADO == 0 ? 'info' : x.FLAG_ESTADO == 1 ? 'success' : 'danger'} p-1"><i class="fas fa-times-circle mr-1"></i>${x.FLAG_ESTADO == 0 ? 'Por habilitar' : x.FLAG_ESTADO == 1 ? 'Habilitado' : 'Deshabilitado'}</div></td>`;
            let btnEditar = `<a class="dropdown-item btnEditar" href="javascript:void(0)" data-id="${x.ID_USUARIO}" data-toggle="modal" data-target="#modal-mantenimiento"><i class="fas fa-edit mr-1"></i>Editar</a>`;
            let colOpciones = `<td class="text-center text-xs-right" data-encabezado="Acciones"><div class="btn-group"><div class="acciones fase-01 dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fas fa-ellipsis-h"></i></div><div class="dropdown-menu dropdown-menu-right">${btnEditar}</div></div></div></td>`;
            let fila = `<tr>${colNro}${colCodigo}${colNombres}${colApellidos}${colCorreo}${colEstado}${colOpciones}</tr>`;
            return fila;
        }).join('');
    };

    return contenido;
};

var nuevoUsuario = () => {
    $('#frmUsuario').removeData('idUsuario');
    $('.alert-add').html('');
    $('#btnGuardar').show();
    $('#btnGuardar').next().html('Cancelar');
    $('#exampleModalLabel').html('Registro de nuevo usuario <br><small class="text-muted">Complete los siguientes campos para registrar un nuevo usuario</small><small class="text-danger d-block"><strong>(*)&nbsp;</strong>Campos obligatorios</small>');
    $('#txt-user-correo').prop('readonly', false);
    $('.admin-edit').removeClass('d-none');
    limpiarDatosUsuario();
}

var limpiarDatosUsuario = () => {
    $('#txt-user-correo').val('');
    $('#txt-nombre').val('');
    $('#txt-apellido').val('');
    $('#txt-pswd').val('');
    $('#txt-re-pswd').val('');
    $('#rad-01').prop('checked', false);
    $('#rad-02').prop('checked', false);
}

var consultarUsuario = (element) => {
    $('.alert-add').html('');
    $('#btnGuardar').show();
    $('#btnGuardar').next().html('Cancelar');
    $('#exampleModalLabel').html('Actualización de usuario <br><small class="text-muted">Puede cambiar los datos mostrados para actualizar un usuario</small><small class="text-danger d-block"><strong>(*)&nbsp;</strong>Campos obligatorios</small>');
    limpiarDatosUsuario();

    let idUsuario = $(element).attr('data-id');
    let urlUsuario = `${baseUrl}Mantenimiento/ObtenerUsuario?idusuario=${idUsuario}`;

    fetch(urlUsuario)
    .then(r => r.json())
    .then(cargarDatosUsuario)
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
}

var cargarDatosUsuario = (r) => {
    if (r == null) return
    data = r.obj
    $('#frmUsuario').data('idUsuario', data.ID_USUARIO);
    $('#txt-nombre').val(data.NOMBRES);
    $('#txt-apellido').val(data.APELLIDOS);
    $('#txt-user-correo').val(data.CORREO);
    $('#rad-01').prop('checked', data.FLAG_ESTADO == 1 ? true : false);
    $('#rad-02').prop('checked', data.FLAG_ESTADO == 2 ? true : false);
    $('#txt-user-correo').prop('readonly', true);
    $('.admin-edit').addClass('d-none');
}

var verificar = () => {
    $('.alert-add').html('');
    let arr = [];
    if (!(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test($("#txt-user-correo").val()))) arr.push("Ingrese un correo electrónico válido");
    if ($('#txt-nombre').val().trim() === "") arr.push("Debe ingresar el/los nombre(s)");
    if ($('#txt-apellido').val().trim() === "") arr.push("Debe ingresar el/los apellido(s)");
    if ($("#frmUsuario").data("idUsuario") == null) {
        if ($("#txt-pswd").val().trim() === $("#txt-re-pswd").val().trim()) {
            if ($("#txt-pswd").val().trim().length < 6) arr.push("La contraseña debe contener 6 o más caracteres por seguridad");
        }
        else arr.push("Las contraseñas no coinciden");
    }
    if (!$('#rad-01').prop('checked') && !$('#rad-02').prop('checked')) arr.push('Debe seleccionar un estado');

    if (arr.length > 0) {
        let error = '';
        $.each(arr, function (ind, elem) { error += '<li><span class="mb-0">' + elem + '</li></span>'; });
        error = `<ul>${error}</ul>`;
        $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: error });
        return;
    }

    if ($('#frmUsuario').data('idUsuario') == null) {
        let correo = $('#txt-user-correo').val().trim()
        let urlVerificarCorreo = `${baseUrl}Mantenimiento/VerificarCorreo?correo=${correo}`;

        fetch(urlVerificarCorreo)
        .then(r => r.json())
        .then((data) => {
            if (data.success) {
                $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: "Encontramos que su correo electrónico se encuentra registrado, por favor ingrese otro correo electrónico" });
            } else
                guardarUsuario();
        });
    } else
        guardarUsuario();
    
}

var guardarUsuario = () => {   
    let idUsuario = $('#frmUsuario').data('idUsuario');
    let nombres = $('#txt-nombre').val();
    let apellidos = $('#txt-apellido').val();
    let correo = $('#txt-user-correo').val();
    let contraseña = $('#txt-pswd').val().trim();
    let flagEstado = $('#rad-01').prop('checked') ? '1' : $('#rad-02').prop('checked') ? '2' : '0';

    let url = `${baseUrl}Mantenimiento/GuardarUsuario`;
    let data = { ID_USUARIO: idUsuario == null ? -1 : idUsuario, CONTRASENA: contraseña, NOMBRES: nombres, APELLIDOS: apellidos, CORREO: correo, FLAG_ESTADO: flagEstado, ID_ROL: 1, UPD_USUARIO: idUsuarioLogin };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };

    fetch(url, init)
    .then(r => r.json())
    .then(j => {
        $('.alert-add').html('');
        if (j.success) { $('#btnGuardar').hide(); $('#btnGuardar').next().html('Cerrar'); }
        j.success ? $('.alert-add').alertSuccess({ type: 'success', title: 'BIEN HECHO', message: 'Los datos fueron guardados correctamente.', close: { time: 1000 }, url: `` }) : $('.alert-add').alertError({ type: 'danger', title: 'ERROR', message: 'Inténtelo nuevamente por favor.' });
        if (j.success) $('#btnConsultar')[0].click();
    })
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
}
