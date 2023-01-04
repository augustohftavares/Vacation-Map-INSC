$(document).ready(function () {

    var currentYear = (new Date).getFullYear();

    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        height: 780,
        locale: 'pt',
        selectable: true,
        fixedWeekCount: true,
        firstDay: true,
        displayEventTime: false,
        titleFormat: {
            year: 'numeric',
            month: 'long'
        },
        headerToolbar: {
            start: 'title',
            center: 'pedirFerias listarFerias imprimir',
            end: 'today prev,next'
        },
        customButtons: {
            pedirFerias: {
                text: 'Pedir Férias',
                click: function () {
                    location.href = "/MapaFerias/Pedir_Ferias"
                }
            },
            listarFerias: {
                text: 'Lista',
                click: function () {
                    location.href = "#listaFerias"
                }
            },
            imprimir: {
                text: 'Imprimir lista ' + currentYear,
                click: function () {
                    location.href = "MapaFerias/Imprimir"
                }
            }
        },
        buttonText: {
            today: 'Hoje',
        },
        events: {
            url: '/MapaFerias/GetCalendarEvents',
            method: 'POST',
            error: function () {
                console.log('%c Erro ao carregar os eventos(calendário)', 'background: #FF0000; color: #000;');
            },
            success: function () {
                console.log('%c Eventos carregados com sucesso(calendário)','background: #68FF00; color: #000');
            }
        },
        eventBorderColor: "#fff",
        eventClick: function (info) {

            Swal.fire({
                title: 'Férias de ' + info.event.title,
                html:
                    '<b class="text-success">  Ínicio: </b>' + formatDate(info.event.start) + '<br />' +
                    '<b class="text-danger">  Fim: </b>' + formatDate(info.event.end),
                showClass: {
                    popup: 'animate__animated animate__fadeInDown'
                },
                hideClass: {
                    popup: 'animate__animated animate__fadeOutUp'
                }
            });
        },
        businessHours: [ // 
            {
                daysOfWeek: [1, 2, 3, 4, 5], // Monday, Tuesday, Wednesday, Thursday,Friday
                startTime: '09:00', // 9am
                endTime: '18:00' // 6pm
            },
        ]
    });
    calendar.render();


    /*
     * paramenters(int num)
     * 
     * return string
     */
    function padTo2Digits(num) {
        return num.toString().padStart(2, '0');
    }

    /*
     * paramenters(DATE date)
     * 
     * return date
     */
    function formatDate(date) {
        return [padTo2Digits(date.getDate()), padTo2Digits(date.getMonth() + 1), date.getFullYear(),].join('/');
    }


}); //end document.ready
