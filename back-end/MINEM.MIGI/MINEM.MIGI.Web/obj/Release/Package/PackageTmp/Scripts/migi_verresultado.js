var arrAnioDescripcion = [], equipo_g = ""
var arrGN1_TablaBienes = [], arrGN1_TablaServicios = [], arrGN2 = [], arrGN3 = []
$(document).ready(() => {
    $('#btn-exportar').on('click', (e) => exportarExcel())
    cargarResultado()
})

var exportarExcel = () => {
    $('.inhabilitar').addClass('disabled-etiqueta-a')
    $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i> Cargando...")
    let url = `${baseUrl}Excel/ExportarExcel`
    let data = { TABLA_BIENES: arrGN1_TablaBienes, TABLA_SERVICIOS: arrGN1_TablaServicios, TABLA_RESUMEN: arrGN2, TABLA_ESTIMADO: arrGN3, ARR_ANIOS: arrAnioDescripcion };
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

var cargarResultado = () => {
    let url = `${baseUrl}Resultado/MostrarResultado/${idresultado}`

    $("#preload").html("<i Class='fas fa-spinner fa-spin px-1'></i> Cargando...")
    $('.mostrar').addClass('d-none')
    $('#seccion-cant-estimada').html('')
    fetch(url)
    .then(r => r.json())
    .then(estructurarFiltro)
    .finally(() => {
        $('#seccion-btn').removeClass('d-none')
        $("#preload").html("")
    })
}

var estructurarFiltro = (data) => {
    if (data == null) return
    let tb = data.ID_TIPO_BUSQUEDA
    $.each((data.LISTA_ANIOS), (x, y) => {
        arrAnioDescripcion.push(y.ANIO)
    })
    
    if (tb == 1) {
        estructurarGraficoN1(data.LISTA_GRAFICO)
        estructurarGraficoN3(data.LISTA_GRAFICON3)
    } else {
        estructurarGraficoM8U(data.LISTA_GRAFICO)
    }

}

var estructurarGraficoN1 = (lista) => {
    if (lista == null) return

    let tituloAnio = arrAnioDescripcion.join(' - ')
    let titulo = `${equipo_g} POR REQUERIMIENTO ${tituloAnio}`

    let arr = []
    arrGN2 = []
    arr.push(["Palabra Clave", "Coincidencia"])
    $.each((lista), (x, y) => {
        if (y.LISTA_GRAFICON1.length > 0) {
            let suma = 0
            let palabra = ""
            let arrAnio = []
            y.LISTA_GRAFICON1.map((w, z) => {
                let sumaAnio = 0
                $.each((arrAnioDescripcion), (m, n) => {
                    if (n == w.ANIO)
                        arrAnio.push([n, w.CANTIDAD])
                    else
                        arrAnio.push([n, 0])
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

    let titulo_bienes = `${equipo_g} POR REQUERIMIENTO BIENES ${tituloAnio}`
    let titulo_servicios = `${equipo_g} POR REQUERIMIENTO SERVICIOS ${tituloAnio}`

    let arrGN1_Bienes = [], arrGN1_Servicios = []
    arrGN1_TablaBienes = [], arrGN1_TablaServicios = []

    arrGN1_Bienes.push(["Palabra Clave", "Coincidencia"])
    arrGN1_Servicios.push(["Palabra Clave", "Coincidencia"])

    $.each((lista), (x, y) => {
        if (y.LISTA_GRAFICON1.length > 0) {
            let suma_bienes = 0, suma_servicios = 0
            let palabra = ""
            let arrAnioBienes = [], arrAnioservicios = []
            y.LISTA_GRAFICON1.map((w, z) => {
                if (w.TIPO_REQUERIMIENTO.toUpperCase() == "BIENES") {
                    $.each((arrAnioDescripcion), (m, n) => {
                        if (n == w.ANIO)
                            arrAnioBienes.push(w.CANTIDAD)
                        else
                            arrAnioBienes.push(0)
                    })
                    suma_bienes += w.CANTIDAD
                }
                else if (w.TIPO_REQUERIMIENTO.toUpperCase() == "SERVICIOS") {
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
                TIPO_REQUERIMIENTO: "BIENES",
                TOTAL: suma_bienes,
                ANIOS: arrAnioBienes
            })
            arrGN1_TablaServicios.push({
                HALLAZGO: palabra,
                TIPO_REQUERIMIENTO: "SERVICIOS",
                TOTAL: suma_servicios,
                ANIOS: arrAnioservicios
            })
        }
    })
    $('.mostrar').removeClass('d-none')
    armarTablaGN1('#tbl-gf1-bienes', "BIENES", arrGN1_TablaBienes)
    armarTablaGN1('#tbl-gf1-servicios', "SERVICIOS", arrGN1_TablaServicios)
    armarTablaGN2('#tbl-gf1-resumen', arrGN2)

    google.charts.setOnLoadCallback(drawChart(arrGN1_Bienes, titulo_bienes, 'pie-gn1-bienes'));
    google.charts.setOnLoadCallback(drawChart(arrGN1_Servicios, titulo_servicios, 'pie-gn1-servicios'));
    google.charts.setOnLoadCallback(drawChart(arr, titulo, 'piechartGN1'));
}

var armarTablaGN1 = (id, tipo_req, arr) => {
    let columna_anio = ""
    $.each((arrAnioDescripcion), (x, y) => {
        columna_anio += `<th>${y}</th>`
    })
    $(id).find('thead').html(`<tr><th>HALLAZGO</th><th>${tipo_req}</th>${columna_anio}<th>TOTAL</th>`)

    let content = arr.map((x, y) => {
        let hallazgo = `<td>${x.HALLAZGO}</td>`
        let tipo_requerimiento = `<td>${x.TIPO_REQUERIMIENTO}</td>`
        let total = `<td>${x.TOTAL}</td>`
        let anios = ""
        $.each((x.ANIOS), (m, n) => {
            anios += `<td>${n}</td>`
        })
        return `<tr>${hallazgo}${tipo_requerimiento}${anios}${total}</tr>`
    }).join('')

    $(id).find('tbody').html(content)
}

var armarTablaGN2 = (id, arr) => {
    let columna_anio = ""
    $.each((arrAnioDescripcion), (x, y) => {
        columna_anio += `<th>${y}</th>`
    })
    $(id).find('thead').html(`<tr><th>HALLAZGO</th>${columna_anio}<th>TOTAL</th>`)

    let content = arr.map((x, y) => {
        let hallazgo = `<td>${x.HALLAZGO}</td>`
        let total = `<td>${x.TOTAL}</td>`
        let anios = ""
        $.each((arrAnioDescripcion), (m, n) => {
            let suma = 0
            $.each((x.ANIOSAR), (a, b) => {
                if (n == b[0])
                    suma += b[1]
            })
            anios += `<td>${suma}</td>`
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
    //    content = lista.map((x, y) => {
    //        let hallazgo
    //        let anios = x.LISTA_GRAFICON3.length == 0 ? '' : x.LISTA_GRAFICON3.map((m, n) => {
    //            hallazgo = m.HALLAZGO
    //            return `<td>${m.CANTIDAD}</td>`
    //        }).join('')
    //        return `<tr><td>${hallazgo}</td>${anios}</tr>`
    //    }).join('')
    //}
    //$('#tbl-gf3-resumen').find('tbody').html(content)

    if (lista.length > 0) {
        lista.map((x, y) => {
            let hallazgo, arrAnioTablaEstimado = []
            x.LISTA_GRAFICON3.map((m, n) => {
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
        lista.map((x, y) => {
            let arrAnio = []
            x.LISTA_GRAFICON3.length == 0 ? '' : x.LISTA_GRAFICON3.map((x, y) => {
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
        arr.push(['Hallazgo', 'cantidad'])
        $('#seccion-cant-estimada').append(`<div class="col-6 mostrar"><div id="piechar-${n}" style="height: 600px;"></div></div>`)
        let titulo = `RESUMEN CANTIDAD ESTIMADA DE ${equipo_g} (${n})`
        $.each((arrListaAnio), (x, y) => {
            $.each((y.LISTA_ANIO), (a, b) => {
                if (n == b.ANIO) arr.push([b.HALLAZGO, b.CANTIDAD])
            })
        })
        google.charts.setOnLoadCallback(drawChart(arr, titulo, `piechar-${n}`));
    })

}

var estructurarGraficoM8U = (lista) => {
    if (lista == null) return
    let arrTipoRequerimiento = []
    arrTipoRequerimiento.push('BIENES')
    arrTipoRequerimiento.push('SERVICIOS')

    let tituloAnio = arrAnioDescripcion.join(' - ')
    let titulo = `${equipo_g} POR REQUERIMIENTO ${tituloAnio}`
    let titulo_bienes = `${equipo_g} POR REQUERIMIENTO BIENES ${tituloAnio}`
    let titulo_servicios = `${equipo_g} POR REQUERIMIENTO SERVICIOS ${tituloAnio}`

    let arrGN1_Bienes = [], arrGN1_Servicios = []
    arrGN1_TablaBienes = [], arrGN1_TablaServicios = []
    arrGN1_Bienes.push(["Palabra Clave", "Coincidencia"])
    arrGN1_Servicios.push(["Palabra Clave", "Coincidencia"])

    let fg1 = lista.length == 0 ? '' : lista.map((x, y) => {
        //palabras
        //let palabra = ""
        let suma_bienes = 0, suma_servicios = 0
        let arrAnioBienes = [], arrAnioservicios = []
        $.each((arrAnioDescripcion), (i, j) => {
            $.each((arrTipoRequerimiento), (p, q) => {
                let suma = 0
                let content = x.LISTA_GRAFICOM8U.length == 0 ? '' : x.LISTA_GRAFICOM8U.map((m, n) => {
                    let contentanio = m.LISTA_GRAFICON1M8U.length == 0 ? '' : m.LISTA_GRAFICON1M8U.map((a, b) => {
                        if (j == a.ANIO) {
                            if (q == a.TIPO_REQUERIMIENTO) {
                                suma += a.CANTIDAD
                            }
                        }
                        //palabra = a.HALLAZGO
                    })
                })
                if (q == "SERVICIOS") {
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
    arrGN2 = []
    arrGN3 = []
    arr.push(["Palabra Clave", "Coincidencia"])
    let fg2 = lista.length == 0 ? '' : lista.map((x, y) => {
        //palabras
        let palabra = ""
        let suma_total = 0
        let arrAnio = [], arrAnioTablaEstimado = [], arrAnioGraficoEstimado = []
        $.each((arrAnioDescripcion), (i, j) => {
            let suma = 0, suma_estimado = 0
            let content = x.LISTA_GRAFICOM8U.length == 0 ? '' : x.LISTA_GRAFICOM8U.map((m, n) => {
                let contentanio = m.LISTA_GRAFICON1M8U.length == 0 ? '' : m.LISTA_GRAFICON1M8U.map((a, b) => {
                    if (j == a.ANIO) {
                        suma += a.CANTIDAD
                        suma_estimado += a.CANTIDAD_ITEM
                    }
                })
            })
            arrAnio.push([j, suma]) //Grafico 2
            arrAnioTablaEstimado.push([j, suma_estimado]) //Tabla 3
            arrAnioGraficoEstimado.push({ //Grafico 3
                HALLAZGO: x.HALLAZGO,
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
    let h4 = '<h4 class="text-center mb-3">RESUMEN CANTIDAD ESTIMADA</h4>'
    let div = `<div class="col-12 mostrar">${h4}${tabla}</div>`
    $('#seccion-cant-estimada').append(div)

    let columna_anio = ""
    $.each((arrAnioDescripcion), (x, y) => {
        columna_anio += `<th>${y}</th>`
    })
    $(id).find('thead').html(`<tr><th>HALLAZGO</th>${columna_anio}`)

    let content = arr.map((x, y) => {
        let hallazgo = `<td>${x.HALLAZGO}</td>`
        let anios = ""
        $.each((arrAnioDescripcion), (m, n) => {
            let suma = 0
            $.each((x.ANIOSAR), (a, b) => {
                if (n == b[0])
                    suma += b[1]
            })
            anios += `<td>${suma}</td>`
        })

        return `<tr>${hallazgo}${anios}</tr>`
    }).join('')

    $(id).find('tbody').html(content)
}

var armarGraficoN3 = (arrListaAnio) => {
    $.each((arrAnioDescripcion), (m, n) => {
        let arr = []
        arr.push(['Hallazgo', 'cantidad'])
        $('#seccion-cant-estimada').append(`<div class="col-6 mostrar"><div id="piechar-${n}" style="height: 600px;"></div></div>`)
        let titulo = `RESUMEN CANTIDAD ESTIMADA DE ${equipo_g} (${n})`
        $.each((arrListaAnio), (x, y) => {
            $.each((y.LISTA_ANIO), (a, b) => {
                if (n == b.ANIO) arr.push([b.HALLAZGO, b.CANTIDAD])
            })
        })
        google.charts.setOnLoadCallback(drawChart(arr, titulo, `piechar-${n}`));
    })
}

var dynamicColors = function () { //metodo para obtener un color aleatorio rgb
    var r = Math.floor(Math.random() * 255);
    var g = Math.floor(Math.random() * 255);
    var b = Math.floor(Math.random() * 255);
    return "rgb(" + r + "," + g + "," + b + ")";
};

function drawChart(arr, titulo, id) {
    var data = google.visualization.arrayToDataTable(arr);

    var options = {
        title: titulo,
        is3D: true,
        legend: 'top',
        tooltip: {
            isHtml: true,
        },
        isStacked: true,
    };

    var chart = new google.visualization.PieChart(document.getElementById(id));
    chart.draw(data, options);
}