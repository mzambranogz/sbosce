var idEliminar = 0
$(document).ready(() => {
    $('.b-activo').removeClass('nav-active')
    $('.v-resultado').addClass('nav-active')
    cargarExcel()
    $('#btnConfirmar').on('click', (e) => eliminar());
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

    let registros = $('#catidad-rgistros').val();
    let pagina = $('#ir-pagina').val();
    let columna = $("#columna").val();
    let orden = $("#orden").val();
    let params = { registros, pagina, columna, orden };
    let queryParams = Object.keys(params).map(x => params[x] == null ? x : `${x}=${params[x]}`).join('&');

    //let url = `${baseUrl}Mantenimiento/BuscarPalabrasClaves?${queryParams}`;

    let url = `${baseUrl}Resultado/ListaResultado?${queryParams}`;
    fetch(url).then(r => r.json()).then(j => {
        if (j == null) return
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
                let colNro = `<td class="text-center" data-encabezado="Código">${++cont}</td>`;
                let colNombres = `<td class="text-center" data-encabezado="Descripción">Resultado N°${x.ID_RESULTADO}</td>`;
                let colFecha = `<td class="text-center" data-encabezado="Descripción">${x.FECHA_REGISTRO}</td>`;
                let btnCambiarEstado = `<a class="dropdown-item estilo-01 btnCambiarEstado" href="javascript:void(0)" data-id="${x.ID_RESULTADO}" data-estado="${x.FLAG_ESTADO}"><i class="fas fa-edit mr-1"></i>Eliminar</a>`;
                let btnEditar = `<a class="dropdown-item estilo-01 btnEditar" href="${baseUrl}Resultado/VerResultado/${x.ID_RESULTADO}" data-id="${x.ID_RESULTADO}"><i class="fas fa-edit mr-1"></i>Ver resultado</a>`;
                let colOpciones = `<td class="text-center" data-encabezado="Gestión"><div class="btn-group w-100"><a class="btn btn-sm bg-success text-white w-100 dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" tabindex="0">Gestionar</a><div class="dropdown-menu">${btnCambiarEstado}${btnEditar}</div></div></td>`;
                //let colOpciones = `<td class="text-center" data-encabezado="Gestión"><div class="btn-group w-100"><a class="btn btn-sm bg-success text-white w-100 dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" tabindex="0">Gestionar</a><div class="dropdown-menu">${btnEditar}</div></div></td>`;
                let fila = `<tr>${colNro}${colNombres}${colFecha}${colOpciones}</tr>`;
                return fila;
            }).join('');

            tabla.find('tbody').html(contenido)

            tabla.find('.btnCambiarEstado').each(x => {
                let elementButton = $('#tbl-excel').find('.btnCambiarEstado')[x];
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

var cambiarEstado = (element) => {
    idEliminar = $(element).attr('data-id');
    $("#modal-confirmacion").modal('show');
};

var eliminar = () => {
    if (idEliminar == 0) return;
    let data = { ID_RESULTADO: idEliminar, UPD_USUARIO: idUsuarioLogin };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };
    let url = `${baseUrl}Resultado/EliminarResultado`;
    fetch(url, init)
        .then(r => r.json())
        .then(j => {
            if (j) { cargarExcel(); $("#modal-confirmacion").modal('hide'); }
        });
}