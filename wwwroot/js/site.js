// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.




// Para busqueda Ajax de usuario
function selectUsuario(selectId, idOriginal, nombreOriginal) {
    $(`#${selectId}`).select2({
        language: "es",
        placeholder: "Buscar por nombre o DNI...",
        minimumInputLength: 2,
        ajax: {
            delay: 300,
            dataType: "json",
            cache: true,
            url: function (params) {
                let q = params.term ? encodeURIComponent(params.term) : "";
                return `/api/Usuarios/buscar?q=${q}`;
            },
            processResults: function (res) {
                return {
                    results: res.map(u => ({
                        id:   u.id,
                        text: `${u.nombre} — DNI: ${u.dni}`
                    }))
                };
            }
        }
    });

    // Pre-selecciona el valor actual si estás editando
    if (idOriginal && nombreOriginal) {
        var option = new Option(nombreOriginal, idOriginal, true, true);
        $(`#${selectId}`).append(option).trigger('change');
    }
}