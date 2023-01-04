/*
 * paramenters: none
 * 
 * return null;
 */ 
function searchBar() {

    //variaveis eventos
    var inputEvents, filterEvents, tableEvents, trEvents, tdEvents;

    inputEvents = document.getElementById("searchEvents");
    filterEvents = inputEvents.value.toUpperCase();
    tableEvents = document.getElementById("tableEvents");
    trEvents = tableEvents.getElementsByTagName("tr");

    for (i = 0; i < trEvents.length; i++) {

        tdEvents = trEvents[i].getElementsByTagName("td")[0];

        if (tdEvents) {
            txtValue = tdEvents.textContent || tdEvents.innerText;
            if (txtValue.toUpperCase().indexOf(filterEvents) > -1)
                trEvents[i].style.display = "";
            else
                trEvents[i].style.display = "none";
        }//end if
    }//end for
}// end searchBar();

/*
 * paramenters: none
 * 
 * return null;
 */ 
function searchBarUsers() {

    //variaveis glob.
    var i, txtValue;

    //variaveis utilizadores
    var inputUsers, filterUsers, tableUsers, trUsers, tdUsers;

    inputUsers = document.getElementById("searchUsers");
    filterUsers = inputUsers.value.toUpperCase();
    tableUsers = document.getElementById("tableUsers");
    trUsers = tableUsers.getElementsByTagName("tr");

    for (i = 0; i < trUsers.length; i++) {

        tdUsers = trUsers[i].getElementsByTagName("td")[2];

        if (tdUsers) {

            txtValue = tdUsers.textContent || tdUsers.innerText;

            if (txtValue.toUpperCase().indexOf(filterUsers) > -1) {

                trUsers[i].style.display = "";

            } else {

                trUsers[i].style.display = "none";

            }

        }//end if

    }//end for

}//end searchBarUsers();