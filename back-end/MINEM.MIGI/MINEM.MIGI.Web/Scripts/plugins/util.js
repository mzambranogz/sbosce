/****************************
MENSAJES
****************************/

//Mensaje de error
//tipomensaje => 1: uno, 2: lista
var mensajeError = (nombreerror, mensaje, tipomensaje) => {
    let contenido = tipomensaje == 1 ? `<small class="mb-0">${mensaje}</small>` : mensaje

    let divwrapbody = `<div class="alert-wrap"><h6>${nombreerror}</h6><hr>${contenido}</div>`

    let divwrapheadersaerror = `<div class="sa-error"><div class="sa-error-x"><div class="sa-error-left"></div><div class="sa-error-right"></div></div><div class="sa-error-placeholder"></div><div class="sa-error-fix"></div></div>`
    let divwrapheadersa = `<div class="sa">${divwrapheadersaerror}</div>`
    let divwrapheader = `<div class="alert-wrap mr-3">${divwrapheadersa}</div>`

    let divinicial = `<div class="alert alert-danger d-flex align-items-stretch" role="alert">${divwrapheader}${divwrapbody}</div>`
    return $(divinicial)
}

//Mensaje de success
var mensajeSuccess = (mensaje) => {
    let divwrapbody = `<div class="alert-wrap"><h6>Bien hecho</h6><hr><small class="mb-0">${mensaje}</small></div>`

    let divwrapheadersasuccess = `<div class="sa-success"><div class="sa-success-tip"></div><div class="sa-success-long"></div><div class="sa-success-placeholder"></div><div class="sa-success-fix"></div></div>`
    let divwrapheadersa = `<div class="sa">${divwrapheadersasuccess}</div>`

    let divwrapheader = `<div class="alert-wrap mr-3">${divwrapheadersa}</div>`

    let divinicial = `<div class="alert alert-success d-flex align-items-stretch" role="alert">${divwrapheader}${divwrapbody}</div>`
    return divinicial
}

//Contenido mensaje de Error
var armarMensajeError = (arr) => {
    let retorno = true, error = ''
    if (arr.length > 0) {
        if (arr.length > 1) {
            $.each(arr, function (ind, elem) { error += '<li><small class="mb-0">' + elem + '</small></li>'; });
            error = `<ul>${error}</ul>`;
        } else
            error = arr[0]        
    } else
        retorno = false
    return {
        Error: retorno,
        Mensaje: error
    }
}

/*
Método para asignar formato miles a números enteros
*/
function formatoMilesEnteros(n) {
    return parseFloat(n).toFixed(0).replace(/\B(?=(\d{3})+(?!\d)\.?)/g, ",");
}

/*
Método para asignar formato miles a números decimales
*/
var formatoMilesDecimales = (n) => {
    return parseFloat(n).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}