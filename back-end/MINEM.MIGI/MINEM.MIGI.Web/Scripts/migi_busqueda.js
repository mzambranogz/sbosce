var arrAnioDescripcion = [], equipo_g
var arrGN1_TablaBienes = [], arrGN1_TablaServicios = [], arrGN2 = [], arrGN3 = []
$(document).ready(() => {
    $('.b-activo').removeClass('nav-active')
    $('.v-busqueda').addClass('nav-active')
    $('#filtrar-datos').on('click', (e) => filtrarInformacion());
    $('#cbo-equipo').on('change', (e) => filtrarPalabras());
    $('#tipo-busqueda').on('change', (e) => cambiarTipoBusqueda())
    $('#btn-exportar').on('click', (e) => exportarExcel())
    $('#btn-guardar').on('click', (e) => guardarResultado())
    $('#btn-nuevo').on('click', (e) => nuevaBusqueda())
    $('#chk-todos-palabras').on('change', (e) => todosPalabras())
    $('#chk-todos-palabras-cantidad').on('change', (e) => todosPalabrasCantidad())
    $('#chk-todos-anios').on('change', (e) => todosAnios())
    cargarComponentes()
});

var todosPalabras = () => {
    let t = $('#chk-todos-palabras').prop('checked')
    if (t) $('#lista-palabras input[type="checkbox"]').prop('checked', true)
    else $('#lista-palabras input[type="checkbox"]').prop('checked', false)
}

var todosPalabrasCantidad = () => {
    let t = $('#chk-todos-palabras-cantidad').prop('checked')
    if (t) $('#lista-palabras-cantidad input[type="checkbox"]').prop('checked', true)
    else $('#lista-palabras-cantidad input[type="checkbox"]').prop('checked', false)
}

var todosAnios = () => {
    let t = $('#chk-todos-anios').prop('checked')
    if (t) $('#lista-anios input[type="checkbox"]').prop('checked', true)
    else $('#lista-anios input[type="checkbox"]').prop('checked', false)
}

var exportarExcel = () => {
    $('.inhabilitar').addClass('disabled-etiqueta-a')
    $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i> Espere por favor, se están exportando los datos...")
    let url = `${baseUrl}Excel/ExportarExcel`
    let data = { TABLA_BIENES: arrGN1_TablaBienes, TABLA_SERVICIOS: arrGN1_TablaServicios, TABLA_RESUMEN: arrGN2, TABLA_ESTIMADO:arrGN3, ARR_ANIOS: arrAnioDescripcion };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };
    fetch(url, init)
    .then(r => r.blob())
    .then(j => {
        let urlBlob = window.URL.createObjectURL(j);
        window.open(urlBlob, '_blank');
    })
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
    .finally(() => {
        $("#preload").html('')
        $('.inhabilitar').removeClass('disabled-etiqueta-a')
    })
}

var guardarResultado = () => {
    $('.inhabilitar').addClass('disabled-etiqueta-a')
    $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i> Los resultados se estan guardando...")
    let url = `${baseUrl}Busqueda/GuardarResultado`
    fetch(url)
    .then(r => r.json())
    .then(j => {
        if (j) {
            $('#seccion-msj').html(mensajeSuccess('Se guardaron correctamente los resultados'))
            setTimeout(() => {
                $('#seccion-msj').html('')
            }, 3500);
            nuevaBusqueda()
        } else {
            $('#seccion-msj').html(mensajeError('Error al guardar', 'Ocurrió un problema al guardar los resultados, inténtelo nuevamente', 1))
            setTimeout(() => {
                $('#seccion-msj').html('')
            }, 3500);
        }
    })
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
    .finally(() => {
        $("#preload").html('')
        $('.inhabilitar').removeClass('disabled-etiqueta-a')
    })
}

var nuevaBusqueda = () => {
    $('#seccion-btn').addClass('d-none')
    $('#filtrar-datos').removeClass('disabled-etiqueta-a')
    $('#tipo-busqueda').prop('disabled', false)
    $('#cbo-equipo').prop('disabled', false)
    $('#lista-palabras input[type="checkbox"]').prop('disabled', false)
    $('#lista-palabras-cantidad input[type="checkbox"]').prop('disabled', false)
    $('#lista-anios input[type="checkbox"]').prop('disabled', false)
    $('#lista-palabras input[type="checkbox"]').prop('checked', false)
    $('#lista-palabras-cantidad input[type="checkbox"]').prop('checked', false)
    $('#lista-anios input[type="checkbox"]').prop('checked', false)
    $('#chk-todos-palabras').prop('disabled', false)
    $('#chk-todos-palabras').prop('checked', false)
    $('#chk-todos-palabras-cantidad').prop('disabled', false)
    $('#chk-todos-palabras-cantidad').prop('checked', false)
    $('#chk-todos-anios').prop('disabled', false)
    $('#chk-todos-anios').prop('checked', false)
    $('.mostrar').addClass('d-none')
    $('#seccion-cant-estimada').html('')
}

var cambiarTipoBusqueda = () => {
    let tipobusqueda = $('#tipo-busqueda').val()
    if (tipobusqueda == 1 || tipobusqueda == 0){        
        $('#lista-palabras-cantidad input[type="checkbox"]').prop('disabled', false)        
        $('#chk-todos-palabras-cantidad').prop('disabled', false)
        $('#lista-palabras-cantidad').parent().parent().removeClass('d-none')
    } else{
        $('#lista-palabras-cantidad input[type="checkbox"]').prop('disabled', true)
        $('#lista-palabras-cantidad input[type="checkbox"]').prop('checked', false)
        $('#chk-todos-palabras-cantidad').prop('disabled', true)
        $('#chk-todos-palabras-cantidad').prop('checked', false)
        $('#lista-palabras-cantidad').parent().parent().addClass('d-none')
    }
}

var cargarComponentes = () => {
    let urlListarEquipos = `${baseUrl}Mantenimiento/ListarEquipos`;
    let urlListarPalabras = `${baseUrl}Busqueda/ListarPalabrasClaves`;
    let urlListarPalabrasCantidad = `${baseUrl}Busqueda/ListarPalabrasClavesCantidad`;
    let urlListarAnios = `${baseUrl}Busqueda/ListarAnios`;    
    Promise.all([
        fetch(urlListarEquipos),
        fetch(urlListarPalabras),
        fetch(urlListarPalabrasCantidad),
        fetch(urlListarAnios),
    ])
    .then(r => Promise.all(r.map(v => v.json())))
    .then(([listaEquipo, listaPalabras, listaPalabrasCantidad, listaAnio]) => {
        //cargarListaEquipos(listaEquipo);
        //cargarListaPalabras(listaPalabras);
        //cargarListaAnios(listaAnio);
        cargarDatos(listaEquipo, listaPalabras, listaPalabrasCantidad, listaAnio)
    })
}

var cargarDatos = (listaEquipo, listaPalabras, listaPalabrasCantidad, listaAnio) => {
    let items = listaEquipo.length == 0 ? '' : listaEquipo.map(x => `<option value="${x.ID_EQUIPO}">${x.EQUIPO}</option>`).join('');
    $('#cbo-equipo').html(`<option value="0">Seleccione un equipo</option>${items}`);

    let chekPalabras = listaPalabras.map((x,y) => {
        let check = '<div class="col-auto my-1"><div class="custom-control custom-checkbox mr-sm-2">'
        check += `<input class="custom-control-input" type="checkbox" id="p-${x.ID_PALABRA}" data-valor="${x.PALABRA}" data-equipo="${x.ID_EQUIPO}" >&nbsp;`
        check += `<label class="custom-control-label" for="p-${x.ID_PALABRA}">${x.PALABRA}</label>`
        check += `</div></div>`;
        return check;
    }).join('')
    $("#lista-palabras").html(chekPalabras);

    let chekPalabrasCantidad = listaPalabrasCantidad.map((x,y) => {
        let check = '<div class="col-auto my-1"><div class="custom-control custom-checkbox mr-sm-2">'
        check += `<input class="custom-control-input" type="checkbox" id="pc-${x.ID_PALABRA_CANTIDAD}" data-valor="${x.PALABRA_CANTIDAD}" >&nbsp;`
        check += `<label class="custom-control-label" for="pc-${x.ID_PALABRA_CANTIDAD}">${x.PALABRA_CANTIDAD}</label>`
        check += `</div></div>`;
        return check;
    }).join('')
    $("#lista-palabras-cantidad").html(chekPalabrasCantidad);

    let chekAnios = listaAnio.map((x,y) => {
        let check = '<div class="col-auto my-1"><div class="custom-control custom-checkbox mr-sm-2">'
        check += `<input class="custom-control-input" type="checkbox" id="a-${x.ID_ANIO}" data-valor="${x.ANIO}">&nbsp;&nbsp;`
        check += `<label class="custom-control-label" for="a-${x.ID_ANIO}">${x.ANIO}</label>`
        check += `</div></div>`;
        return check;
    }).join('')
    $("#lista-anios").html(chekAnios);

    filtrarPalabras();
}

//var cargarListaEquipos = (lista) => {
//    let items = lista.length == 0 ? '' : lista.map(x => `<option value="${x.ID_EQUIPO}">${x.EQUIPO}</option>`).join('');
//    $('#cbo-equipo').html(`<option value="0">Seleccione un equipo</option>${items}`);
//}

//var cargarListaPalabras = (lista) => {
//    let chekPalabras = lista.map((x,y) => {
//        let check = '<div class="col-auto my-1"><div class="custom-control custom-checkbox mr-sm-2">'
//        check += `<input class="custom-control-input" type="checkbox" id="p-${x.ID_PALABRA}" data-valor="${x.PALABRA}" data-equipo="${x.ID_EQUIPO}" >&nbsp;`
//        check += `<label class="custom-control-label" for="p-${x.ID_PALABRA}">${x.PALABRA}</label>`
//        check += `</div></div>`;
//        return check;
//    }).join('')
//    $("#lista-palabras").html(chekPalabras);
//}

//var cargarListaAnios = (lista) => {
//    let chekAnios = lista.map((x,y) => {
//        let check = '<div class="col-auto my-1"><div class="custom-control custom-checkbox mr-sm-2">'
//        check += `<input class="custom-control-input" type="checkbox" id="a-${x.ID_ANIO}" data-valor="${x.ANIO}">&nbsp;&nbsp;`
//        check += `<label class="custom-control-label" for="a-${x.ID_ANIO}">${x.ANIO}</label>`
//        check += `</div></div>`;
//        return check;
//    }).join('')
//    $("#lista-anios").html(chekAnios);
//}

var filtrarPalabras = () => {
    let equipo = $('#cbo-equipo').val()
    if ( equipo == 0) {
        $('[id*="p-"]').each((x,y) => {
            $(y).parent().parent().addClass('d-none')
            $(y).prop('checked', false)
        })
        $('#chk-todos-palabras').prop('checked', false)
        return
    }

    $('[id*="p-"]').each((x,y) => {
        $(y).prop('checked', false)
        if ($(y).data("equipo") == equipo)
            $(y).parent().parent().removeClass('d-none')
        else
            $(y).parent().parent().addClass('d-none')
    })
}

var filtrarInformacion = () => {
    $('.alert-add').html('')
    arrAnioDescripcion = []
    let arrPalabras = [], arrPalabrasCantidad = [], arrAnios = [], arr = []
    let tb = $('#tipo-busqueda').val();
    $('[id*="p-"]').each((x,y) => {
        let idPalabra = $(y)[0].id;
        if ($(`#${idPalabra}`).prop('checked')){
            arrPalabras.push({
                ID_PALABRA: idPalabra.replace('p-',''),
                PALABRA: $(`#${idPalabra}`).data("valor")
            })            
        }            
    })

    if (tb == 1) {
        $('[id*="pc-"]').each((x,y) => {
            let idPalabra = $(y)[0].id;
            if ($(`#${idPalabra}`).prop('checked')){
                arrPalabrasCantidad.push({
                    ID_PALABRA_CANTIDAD: idPalabra.replace('pc-',''),
                    PALABRA_CANTIDAD: $(`#${idPalabra}`).data("valor")
                })            
            }            
        })
    }    

    $('[id*="a-"]').each((x,y) => {
        let idAnio = $(y)[0].id;
        if ($(`#${idAnio}`).prop('checked')){
            arrAnios.push({
                ID_ANIO: idAnio.replace('a-',''),
                ANIO: $(`#${idAnio}`).data("valor")
            })
            arrAnioDescripcion.push($(`#${idAnio}`).data("valor"))
        }   
    })   
    equipo_g = $('#cbo-equipo option:selected').text()

    if (tb == 0) arr.push('Debe seleccionar un tipo de búsqueda');
    if (arrPalabras.length == 0) arr.push('Debe seleccionar al menos una palabra clave');
    //if (tb == 1) if (arrPalabrasCantidad.length == 0) arr.push('Debe seleccionar al menos una palabra clave de cantidad');
    if (arrAnios.length == 0) arr.push('Debe seleccionar al menos un año');

    //if (arr.length > 0) {
    //    let error = '';
    //    $.each(arr, function (ind, elem) { error += '<li><small class="mb-0">' + elem + '</li></small>'; });
    //    error = `<ul style="color: red; border: 1px solid red;">${error}</ul>`;
    //    $('.alert-add').html(error);
    //    return;
    //}
    let objValidar = armarMensajeError(arr)
    if (objValidar.Error) {
        $('#seccion-msj').html(mensajeError('Error validación', objValidar.Mensaje, arr.length))
        return
    }

    let url = `${baseUrl}Busqueda/FiltrarInformacion`
    let data = { ID_TIPO_BUSQUEDA: tb, LISTA_PALABRAS: arrPalabras, LISTA_PALABRAS_CANTIDAD: arrPalabrasCantidad, LISTA_ANIOS: arrAnios };
    let init = { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };

    $('.inhabilitar').addClass('disabled-etiqueta-a')
    $('#filtrar-datos').addClass('disabled-etiqueta-a')
    $('#tipo-busqueda').prop('disabled', true)
    $('#cbo-equipo').prop('disabled', true)
    $('#lista-palabras input[type="checkbox"]').prop('disabled', true)
    $('#lista-palabras-cantidad input[type="checkbox"]').prop('disabled', true)
    $('#lista-anios input[type="checkbox"]').prop('disabled', true)
    $('#chk-todos-palabras').prop('disabled', true)
    $('#chk-todos-palabras-cantidad').prop('disabled', true)
    $('#chk-todos-anios').prop('disabled', true)
    $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i> Espere unos minutos por favor, se está realizando la búsqueda...")
    $('.mostrar').addClass('d-none')
    $('#seccion-cant-estimada').html('')    
    fetch(url, init)
    .then(r => r.json())
    .then(estructurarFiltro)
    .catch(error => {
        console.log('Hubo un problema con la petición Fetch:' + error.message);
        location.href = `${baseUrl}Inicio/Index`
    })
    .finally(() => {
        $('.inhabilitar').removeClass('disabled-etiqueta-a')
        //$('#filtrar-datos').removeClass('disabled-etiqueta-a')
        //$('#tipo-busqueda').prop('disabled', false)
        //$('#cbo-equipo').prop('disabled', false)
        $('#seccion-btn').removeClass('d-none')
        $("#preload").html("")
    })
}

var estructurarFiltro = (data) => {

    //if (data.LISTA_ORDENCOMPRA == null) return
    //let cont = 0;
    //let content = data.LISTA_ORDENCOMPRA.length == 0 ? '' : data.LISTA_ORDENCOMPRA.map((x,y) => {
    //    cont++;
    //    if (cont > 10) return ''
    //    let entidad = `<td>${x.ENTIDAD}</td>`
    //    let ruc_entidad = `<td>${x.RUC_ENTIDAD}</td>`
    //    let proceso = `<td>${x.ORDEN}</td>`
    //    let descripcion = `<td>${x.DESCRIPCION_ORDEN}</td>`
    //    return `<tr>${entidad}${ruc_entidad}${proceso}${descripcion}</tr>`
    //}).join('');
    //$('#tbl-busqueda').find('tbody').html(content)

    //console.log(data.LISTA_GRAFICON3)
    if (data == null) return
    let tb = $('#tipo-busqueda').val()
    if (tb == 1){
        estructurarGraficoN1(data.LISTA_GRAFICO)
        if (data.LISTA_GRAFICON3 != null) if (data.LISTA_GRAFICON3.length > 0) estructurarGraficoN3(data.LISTA_GRAFICON3)
    } else {
        estructurarGraficoM8U(data.LISTA_GRAFICO)
    }
    
}

var estructurarGraficoN1 = (lista) => {
    if (lista == null) return

    //let content = lista.length == 0 ? '' : lista.map((x,y) => {
    //    return x.LISTA_GRAFICON1.length == 0 ? '' : x.LISTA_GRAFICON1.map((a, b) => {
    //        let hallazgo = `<td>${a.HALLAZGO}</td>`
    //        let tiporequerimiento = `<td>${a.TIPO_REQUERIMIENTO}</td>`
    //        let cantidad = `<td>${a.CANTIDAD}</td>`
    //        let anio = `<td>${a.ANIO}</td>`
    //        return `<tr>${hallazgo}${tiporequerimiento}${cantidad}${anio}</tr>`
    //    }).join('')     
    //}).join('')

    //$('#tbl-grafico-n1').find('tbody').html(content)

    let tituloAnio = arrAnioDescripcion.join(' - ')
    let titulo = `Resumen por requerimiento ${tituloAnio}`

    let arr = [] 
    arrGN2 = []
    arr.push(["Palabra Clave", "Coincidencia"])
    $.each((lista), (x,y) => {
        if (y.LISTA_GRAFICON1.length > 0){
            let suma = 0
            let palabra = ""
            let arrAnio = []
            y.LISTA_GRAFICON1.map((w,z) => {
                let sumaAnio = 0
                $.each((arrAnioDescripcion), (m, n) => {
                    if (n == w.ANIO)
                        arrAnio.push([n , w.CANTIDAD])
                    else
                        arrAnio.push([n , 0])
                })
                suma += w.CANTIDAD
                palabra = w.HALLAZGO
            })
            arr.push([palabra, suma])
            arrGN2.push({
                HALLAZGO: palabra,
                TOTAL: suma,
                ANIOSAR: arrAnio
            })
        } else {

            arr.push(['', 0])
            arrGN2.push({
                HALLAZGO: '',
                TOTAL: 0,
                ANIOSAR: null
            })
        }
    })
    //mostrarGraficoN1(arr, titulo)
    //armarTablaGN2('#tbl-gf1-resumen', arrGN2)
    //google.charts.setOnLoadCallback(drawChart(arr, titulo, 'piechartGN1'));

    let titulo_bienes = `Búsqueda por requerimiento bienes ${tituloAnio}`
    let titulo_servicios = `Búsqueda por requerimiento servicios ${tituloAnio}`

    let arrGN1_Bienes = [], arrGN1_Servicios = []//, arrGN1_TablaBienes = [], arrGN1_TablaServicios = []
    arrGN1_TablaBienes = [] 
    arrGN1_TablaServicios = []
    arrGN1_Bienes.push(["Palabra Clave", "Coincidencia"])
    arrGN1_Servicios.push(["Palabra Clave", "Coincidencia"])

    $.each((lista), (x,y) => {
        if (y.LISTA_GRAFICON1.length > 0){
            let suma_bienes = 0, suma_servicios = 0
            let palabra = ""
            let arrAnioBienes = [], arrAnioservicios = []
            y.LISTA_GRAFICON1.map((w,z) => {
                if (w.TIPO_REQUERIMIENTO.toUpperCase() == "BIENES"){
                    $.each((arrAnioDescripcion), (m, n) => {
                        if (n == w.ANIO)
                            arrAnioBienes.push(w.CANTIDAD)
                        else
                            arrAnioBienes.push(0)
                    })
                    suma_bienes += w.CANTIDAD
                }                    
                else if (w.TIPO_REQUERIMIENTO.toUpperCase() == "SERVICIOS"){
                    $.each((arrAnioDescripcion), (m, n) => {
                        if (n == w.ANIO)
                            arrAnioservicios.push(w.CANTIDAD)
                        else
                            arrAnioservicios.push(0)
                    })
                    suma_servicios += w.CANTIDAD
                }                    
                
                palabra = w.HALLAZGO
            })
            /* Graficos Bienes y servicios */
            arrGN1_Bienes.push([palabra, suma_bienes])
            arrGN1_Servicios.push([palabra, suma_servicios])
            /* Tablas Bienes y servicios */
            arrGN1_TablaBienes.push({
                HALLAZGO: palabra,
                TIPO_REQUERIMIENTO: "Bienes",
                TOTAL: suma_bienes,
                ANIOS: arrAnioBienes
            })
            arrGN1_TablaServicios.push({
                HALLAZGO: palabra,
                TIPO_REQUERIMIENTO: "Servicios",
                TOTAL: suma_servicios,
                ANIOS: arrAnioservicios
            })
        }
    })
    $('.mostrar').removeClass('d-none')
    armarTablaGN1('#tbl-gf1-bienes', "Bienes", arrGN1_TablaBienes)
    armarTablaGN1('#tbl-gf1-servicios', "Servicios", arrGN1_TablaServicios)
    armarTablaGN2('#tbl-gf1-resumen', arrGN2)

    google.charts.setOnLoadCallback(drawChart(arrGN1_Bienes, titulo_bienes, 'pie-gn1-bienes'));
    google.charts.setOnLoadCallback(drawChart(arrGN1_Servicios, titulo_servicios, 'pie-gn1-servicios'));    
    google.charts.setOnLoadCallback(drawChart(arr, titulo, 'piechartGN1'));
}

var armarTablaGN1 = (id, tipo_req, arr) => {
    let columna_anio = ""
    $.each((arrAnioDescripcion), (x, y) => {
        columna_anio += `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">${y}</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="bottom" title="Año ${y}"></i></div></div></th>`
    })

    let colHallazgo = `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">Hallazgo</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="right" title="La palabra clave en ${tipo_req}"></i></div></div></th>`
    let colRequerimiento = `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">${tipo_req}</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="right" title="Requerimiento bienes"></i></div></div></th>`
    let colTotal = `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">Total</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="right" title="Suma total de las cantidades de hallazgos por año de la palabra clave"></i></div></div></th>`
    $(id).find('thead').html(`<tr class="bg-primary text-white">${colHallazgo}${colRequerimiento}${columna_anio}${colTotal}</tr>`)

    let content = arr.map((x,y) => {
        let hallazgo = `<td data-encabezado="Hallazgo">${x.HALLAZGO}</td>`
        let tipo_requerimiento = `<td data-encabezado="${tipo_req}">${x.TIPO_REQUERIMIENTO}</td>`
        let total = `<td class="text-center" data-encabezado="Total">${formatoMilesEnteros(x.TOTAL)}</td>`
        let anios = ""
        $.each((x.ANIOS), (m, n) => {
            anios += `<td class="text-center" data-encabezado="">${formatoMilesEnteros(n)}</td>`
        })
        return `<tr>${hallazgo}${tipo_requerimiento}${anios}${total}</tr>`
    }).join('')

    $(id).find('tbody').html(content)
}

var armarTablaGN2 = (id, arr) => {
    let columna_anio = ""
    $.each((arrAnioDescripcion), (x, y) => {
        columna_anio += `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">${y}</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="bottom" title="Año ${y}"></i></div></div></th>`
    })

    let colHallazgo = `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">Hallazgo</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="right" title="La palabra clave en bienes"></i></div></div></th>`
    let colTotal = `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">Total</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="right" title="Suma total de las cantidades de hallazgos por año de la palabra clave"></i></div></div></th>`

    $(id).find('thead').html(`<tr class="bg-primary text-white">${colHallazgo}${columna_anio}${colTotal}</tr>`)

    let content = arr.map((x,y) => {
        let hallazgo = `<td data-encabezado="Hallazgo">${x.HALLAZGO}</td>`
        let total = `<td class="text-center" data-encabezado="Total">${formatoMilesEnteros(x.TOTAL)}</td>`
        let anios = ""
        $.each((arrAnioDescripcion), (m, n) => {
            let suma = 0
            $.each((x.ANIOSAR), (a, b) => {
                if (n == b[0])
                    suma += b[1]                
            })
            anios += `<td class="text-center" data-encabezado="">${formatoMilesEnteros(suma)}</td>`
        })
        
        return `<tr>${hallazgo}${anios}${total}</tr>`
    }).join('')

    $(id).find('tbody').html(content)
}

var estructurarGraficoN3 = (lista) => {
    if (lista == null) return
    arrGN3 = []
    //let body = '<tbody></tbody>'
    //let head = '<thead></thead>'
    //let tabla = `<table id="tbl-gf3-resumen" class="table table-hover">${head}${body}</table>`
    //let h4 = '<h4 class="text-center mb-3">RESUMEN CANTIDAD ESTIMADA</h4>'
    //let div = `<div class="col-12 mostrar">${h4}${tabla}</div>`
    //$('#seccion-cant-estimada').append(div)

    //let columna_anio = ""
    //$.each((arrAnioDescripcion), (x, y) => {
    //    columna_anio += `<th>${y}</th>`
    //})
    //$('#tbl-gf3-resumen').find('thead').html(`<tr><th>HALLAZGO</th>${columna_anio}`)

    //let content = '';
    //if (lista.length > 0) {
    //    content = lista.map((x,y) => {
    //        let hallazgo
    //        let anios = x.LISTA_GRAFICON3.length == 0 ? '' : x.LISTA_GRAFICON3.map((m,n) => {
    //            hallazgo = m.HALLAZGO
    //            return `<td>${m.CANTIDAD}</td>`
    //        }).join('')
    //        return `<tr><td>${hallazgo}</td>${anios}</tr>`
    //    }).join('')
    //}
    //$('#tbl-gf3-resumen').find('tbody').html(content)

    if (lista.length > 0) {
        lista.map((x,y) => {
            let hallazgo, arrAnioTablaEstimado = []
            x.LISTA_GRAFICON3.map((m,n) => {
                hallazgo = m.HALLAZGO
                arrAnioTablaEstimado.push([m.ANIO, m.CANTIDAD])
            })
            arrGN3.push({ //Tabla 3
                HALLAZGO: hallazgo,
                ANIOSAR: arrAnioTablaEstimado
            })
        })
    }

    armarTablaGN3('#tbl-gf3-resumen', arrGN3)

    let arrListaAnio = []
    $.each((arrAnioDescripcion), (m, n) => {
        lista.map((x,y) => {
            let arrAnio = []
            x.LISTA_GRAFICON3.length == 0 ? '' : x.LISTA_GRAFICON3.map((x,y) => {
                if (n == x.ANIO) {
                    arrAnio.push({
                        HALLAZGO: x.HALLAZGO,
                        ANIO: x.ANIO,
                        CANTIDAD: x.CANTIDAD
                    })
                }                
            })
            arrListaAnio.push({
                LISTA_ANIO: arrAnio
            })
        })
    })

    $.each((arrAnioDescripcion), (m, n) => {
        let arr = []
        arr.push(['Hallazgo','cantidad'])
        $('#seccion-cant-estimada .row').append(`<div class="col-6 mostrar"><div id="piechar-${n}" style="height: 500px;"></div></div>`)
        let titulo = `Resumen de cantidad estimada (${n})`
        $.each((arrListaAnio), (x,y) => {            
            $.each((y.LISTA_ANIO), (a,b) => {
                if (n == b.ANIO) arr.push([b.HALLAZGO, b.CANTIDAD])
            })            
        })
        google.charts.setOnLoadCallback(drawChart(arr, titulo, `piechar-${n}`));
    })
}

var estructurarGraficoM8U = (lista) => {
    if (lista == null) return
    let arrTipoRequerimiento = []
    arrGN1_TablaBienes = []
    arrGN1_TablaServicios = []
    arrTipoRequerimiento.push('BIENES')
    arrTipoRequerimiento.push('SERVICIOS')

    let tituloAnio = arrAnioDescripcion.join(' - ')
    let titulo = `Por requerimiento ${tituloAnio}`
    let titulo_bienes = `Por requerimiento bienes ${tituloAnio}`
    let titulo_servicios = `${equipo_g} Por requerimiento servicios ${tituloAnio}`

    let arrGN1_Bienes = [], arrGN1_Servicios = []//, arrGN1_TablaBienes = [], arrGN1_TablaServicios = []
    arrGN1_Bienes.push(["Palabra Clave", "Coincidencia"])
    arrGN1_Servicios.push(["Palabra Clave", "Coincidencia"])

    let fg1 = lista.length == 0 ? '' : lista.map((x,y) => {
        //palabras
        //let palabra = ""
        let suma_bienes = 0, suma_servicios = 0        
        let arrAnioBienes = [], arrAnioservicios = []
        $.each((arrAnioDescripcion), (i, j) => {
            $.each((arrTipoRequerimiento), (p, q) => {
                let suma = 0
                let content = x.LISTA_GRAFICOM8U.length == 0 ? '' : x.LISTA_GRAFICOM8U.map((m,n) => {
                    let contentanio = m.LISTA_GRAFICON1M8U.length == 0 ? '' : m.LISTA_GRAFICON1M8U.map((a, b) => {
                        if (j == a.ANIO){
                            if (q == a.TIPO_REQUERIMIENTO){
                                suma += a.CANTIDAD
                            }
                        }
                        //palabra = a.HALLAZGO
                    })
                })
                if (q == "SERVICIOS"){
                    suma_servicios += suma
                    arrAnioservicios.push(suma)
                } else if (q == "BIENES") {
                    suma_bienes += suma
                    arrAnioBienes.push(suma)
                }                
            })
        })

        arrGN1_Bienes.push([x.HALLAZGO, suma_bienes])
        arrGN1_Servicios.push([x.HALLAZGO, suma_servicios])
        /* Tablas Bienes y servicios */
        arrGN1_TablaBienes.push({
            HALLAZGO: x.HALLAZGO,
            TIPO_REQUERIMIENTO: "BIENES",
            TOTAL: suma_bienes,
            ANIOS: arrAnioBienes
        })
        arrGN1_TablaServicios.push({
            HALLAZGO: x.HALLAZGO,
            TIPO_REQUERIMIENTO: "SERVICIOS",
            TOTAL: suma_servicios,
            ANIOS: arrAnioservicios
        })
    })
    $('.mostrar').removeClass('d-none')
    armarTablaGN1('#tbl-gf1-bienes', "BIENES", arrGN1_TablaBienes)
    armarTablaGN1('#tbl-gf1-servicios', "SERVICIOS", arrGN1_TablaServicios)
    google.charts.setOnLoadCallback(drawChart(arrGN1_Bienes, titulo_bienes, 'pie-gn1-bienes'));
    google.charts.setOnLoadCallback(drawChart(arrGN1_Servicios, titulo_servicios, 'pie-gn1-servicios'));

    let arrListaAnio = []
    let arr = []
    arrGN3 = []
    arrGN2 = []
    arr.push(["Palabra Clave", "Coincidencia"])
    let fg2 = lista.length == 0 ? '' : lista.map((x,y) => {
        //palabras
        let palabra = ""
        let suma_total = 0        
        let arrAnio = [], arrAnioTablaEstimado = [], arrAnioGraficoEstimado = []
        $.each((arrAnioDescripcion), (i, j) => {
            let suma = 0, suma_estimado = 0
            let content = x.LISTA_GRAFICOM8U.length == 0 ? '' : x.LISTA_GRAFICOM8U.map((m,n) => {
                let contentanio = m.LISTA_GRAFICON1M8U.length == 0 ? '' : m.LISTA_GRAFICON1M8U.map((a, b) => {
                    if (j == a.ANIO){
                        suma += a.CANTIDAD
                        suma_estimado += a.CANTIDAD_ITEM
                    }
                })
            })
            arrAnio.push([j, suma]) //Grafico 2
            arrAnioTablaEstimado.push([j, suma_estimado]) //Tabla 3
            arrAnioGraficoEstimado.push({ //Grafico 3
                HALLAZGO: x.HALLAZGO ,
                ANIO: j,
                CANTIDAD: suma_estimado
            })
            suma_total += suma
            palabra = x.HALLAZGO        
        })
        arr.push([palabra, suma_total]) //Grafico 2
        arrGN2.push({ //Tabla 2
            HALLAZGO: palabra,
            TOTAL: suma_total,
            ANIOSAR: arrAnio
        })
        arrListaAnio.push({ //Grafico 3
            LISTA_ANIO: arrAnioGraficoEstimado
        })
        arrGN3.push({ //Tabla 3
            HALLAZGO: palabra,
            ANIOSAR: arrAnioTablaEstimado
        })
    })

    armarTablaGN2('#tbl-gf1-resumen', arrGN2)
    armarTablaGN3('#tbl-gf3-resumen', arrGN3)
    google.charts.setOnLoadCallback(drawChart(arr, titulo, 'piechartGN1'));
    armarGraficoN3(arrListaAnio)
}

var armarTablaGN3 = (id, arr) => {
    let body = '<tbody></tbody>'
    let head = '<thead></thead>'
    let tabla = `<table id="tbl-gf3-resumen" class="table table-hover">${head}${body}</table>`
    let contenedorTabla = `<div class="table-responsive tabla-principal">${tabla}</div>`
    let titulo = `<div class="h5">Resumen cantidad estimada&nbsp;<i class="fas fa-question-circle ayuda-tooltip" data-toggle="tooltip" data-placement="right" title="Tabla Resumen con la cantidad estimada de unidades por año"></i></div>`    
    let div = `<div class="col-12 mostrar">${titulo}${contenedorTabla}</div>`
    $('#seccion-cant-estimada').append(`<div class="row">${div}</div>`)

    let columna_anio = ""
    $.each((arrAnioDescripcion), (x, y) => {
        columna_anio += `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">${y}</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="bottom" title="Año ${y}"></i></div></div></th>`
    })

    let colHallazgo = `<th scope="col" width=""><div class="d-flex flex-column justify-content-between align-items-center"><div class="d-flex justify-content-center align-items-center"><div class="pl-1">Hallazgo</div></div><div class="d-flex justify-content-center align-items-center"><i class="fas fa-info-circle mr-1" data-toggle="tooltip" data-placement="right" title="Palabra clave"></i></div></div></th>`

    $(id).find('thead').html(`<tr class="bg-primary text-white">${colHallazgo}${columna_anio}`)

    let content = arr.map((x,y) => {
        let hallazgo = `<td data-encabezado="Hallazgo">${x.HALLAZGO}</td>`
        let anios = ""
        $.each((arrAnioDescripcion), (m, n) => {
            let suma = 0
            $.each((x.ANIOSAR), (a, b) => {
                if (n == b[0])
                    suma += b[1]                
            })
            anios += `<td class="text-center" data-encabezado="">${formatoMilesEnteros(suma)}</td>`
        })
        
        return `<tr>${hallazgo}${anios}</tr>`
    }).join('')

    $(id).find('tbody').html(content)
}

var armarGraficoN3 = (arrListaAnio) => {
    $.each((arrAnioDescripcion), (m, n) => {
        let arr = []
        arr.push(['Hallazgo','cantidad'])
        $('#seccion-cant-estimada .row').append(`<div class="col-6 mostrar"><div id="piechar-${n}" style="height: 500px;"></div></div>`)
        let titulo = `Resumen cantidad estimada (${n})`
        $.each((arrListaAnio), (x,y) => {         
            $.each((y.LISTA_ANIO), (a,b) => {
                if (n == b.ANIO) arr.push([b.HALLAZGO, b.CANTIDAD])
            })            
        })
        google.charts.setOnLoadCallback(drawChart(arr, titulo, `piechar-${n}`));
    })
}

var dynamicColors = function() { //metodo para obtener un color aleatorio rgb
    var r = Math.floor(Math.random() * 255);
    var g = Math.floor(Math.random() * 255);
    var b = Math.floor(Math.random() * 255);
    return "rgb(" + r + "," + g + "," + b + ")";
};

function drawChart(arr, titulo, id) {
    //var data = google.visualization.arrayToDataTable([
    //  ['Task', 'Hours per Day'],
    //  ['Work',     11],
    //  ['Eat',      2],
    //  ['Commute',  2],
    //  ['Watch TV', 2],
    //  ['Sleep',    7]
    //]);

    var data = google.visualization.arrayToDataTable(arr);

    //var options = {
    //    title: 'My Daily Activities',
    //    is3D: true,
    //};

    var options = {
        title: titulo,
        is3D: true,
        legend: 'top',
        //width: 900,
        //height: 800,
        tooltip: { 
            isHtml: true,
            //trigger: 'selection'
        },
        //slices: {
        //    0: { color: 'rgb(230,0,0)' },
        //    1: { color: 'yellow' }
        //},
        //bar: { groupWidth: '75%' },
        isStacked: true,
    };

    var chart = new google.visualization.PieChart(document.getElementById(id));
    chart.draw(data, options);
}