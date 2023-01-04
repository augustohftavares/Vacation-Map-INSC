$(document).ready(function () {

    const el = document.getElementById('status');
    const box = document.getElementById('options');

    el.addEventListener('change', function handleChange(event) {

        if (event.target.value === 'Aprovado') {
            box.style.visibility = 'hidden';
        } else {
            box.style.visibility = 'visible';
        }
    });

})//end document ready