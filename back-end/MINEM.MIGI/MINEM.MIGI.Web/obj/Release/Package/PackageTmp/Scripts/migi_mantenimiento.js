
$(document).ready(() => {
    $('.b-activo').removeClass('nav-active')
    $('.v-mantenimiento').addClass('nav-active')
    $('#ir-mantenimiento').on('click', (e) => irMantenimiento(e))
})

var irMantenimiento = (e) => {
    e.preventDefault();
    let valorMantenimiento = $("#cbo-tabla-mantenimiento").val();
    let urlMantenimiento = $("#cbo-tabla-mantenimiento option:checked").attr("data-url");
    let redirectUrl = `${baseUrl}${urlMantenimiento}`;

    if (valorMantenimiento != "0") {
        location.href = redirectUrl;
    }
}