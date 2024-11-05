
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/callHub")
    .build();

connection.start().then(() => {
    console.log("Connected to SignalR Hub");
});

connection.on("ReceiveCall", (aspNetUserId) => {

    console.log("modal show");
    $('#requestModal').modal('show');
    var id = "joinCall" + aspNetUserId;
    console.log("id ::::", id);
    if (document.getElementById(id) != null) {
        document.getElementById(id).style.display = "block";
    }
    console.log("Call started by " + aspNetUserId);

    $.ajax({
        method: "PUT",
        url: "/Home/UpdateHostCallStatus",
        data: {
            AspNetUserId: aspNetUserId,
            status: true,
        },
        success: function (response) {

        },
        error: function () {
            console.log("Function Fail")
        }
    });


});

connection.on("CloseCall", (aspNetUserId) => {

    console.log("modal hide");
    $('#requestModal').modal('hide');
    var id = "joinCall" + aspNetUserId;
    if (document.getElementById(id) != null) {
        document.getElementById(id).style.display = "none";
    }
    console.log("Call ended by " + aspNetUserId);

    $.ajax({
        method: "PUT",
        url: "/Home/UpdateHostCallStatus",
        data: {
            AspNetUserId: aspNetUserId,
            status: false,
        },
        success: function (response) {

        },
        error: function () {
            console.log("Function Fail")
        }
    });
});

connection.on("JoinRequest", (participantId) => {
    //const accept = confirm(`Participant ${participantId} wants to join the call. Accept?`);
    //connection.invoke("RespondToRequest", participantId, accept);
    //document.getElementById("joinCall").style.display = "none";

        const buttonContainer = document.createElement("div");
        buttonContainer.className = "d-flex gap-2";

        console.log("connection on joinRequest");
        const requestList = document.getElementById("requestList");
        const listItem = document.createElement("li");

        listItem.className = "list-group-item";
        listItem.textContent = `Participant ${participantId} wants to join. `;


        const acceptButton = document.createElement("button");
        acceptButton.className = "btn btn-success btn-sm ml-2";
        acceptButton.textContent = "Accept";

        acceptButton.onclick = function () {
            connection.invoke("RespondToRequest", participantId, true);
            requestList.removeChild(listItem);
        };


        const declineButton = document.createElement("button");
        declineButton.className = "btn btn-danger btn-sm ml-2";
        declineButton.textContent = "Decline";
        declineButton.onclick = function () {
            connection.invoke("RespondToRequest", participantId, false);
            requestList.removeChild(listItem);
        };

        buttonContainer.appendChild(acceptButton);
        buttonContainer.appendChild(declineButton);
        listItem.appendChild(buttonContainer);
        requestList.appendChild(listItem);
        requestList.appendChild(listItem);
   
});

connection.on("RequestAccepted", (participantId) => {
    alert("Your request to join the call has been accepted!", participantId);
});

connection.on("RequestDeclined", (participantId) => {

    alert("Your request to join the call has been declined.", participantId);
});

//connection.on("CallStarted", (hostId) => {
//    console.log("modal show");
//    $('#requestModal').modal('show');
//});

//connection.on("JoinRequest", (participantId) => {
//    console.log("connection on joinRequest");
//    const requestList = document.getElementById("requestList");
//    const listItem = document.createElement("li");

//    listItem.className = "list-group-item";
//    listItem.textContent = `Participant ${participantId} wants to join. `;


//    const acceptButton = document.createElement("button");
//    acceptButton.className = "btn btn-success btn-sm ml-2";
//    acceptButton.textContent = "Accept";

//    acceptButton.onclick = function () {
//        connection.invoke("AcceptCall", participantId);
//        requestList.removeChild(listItem);
//    };


//    const declineButton = document.createElement("button");
//    declineButton.className = "btn btn-danger btn-sm ml-2";
//    declineButton.textContent = "Decline";
//    declineButton.onclick = function () {
//        connection.invoke("DeclineCall", participantId);
//        requestList.removeChild(listItem);
//    };

//    listItem.appendChild(acceptButton);
//    listItem.appendChild(declineButton);
//    requestList.appendChild(listItem);
//});

//connection.on("CallStarted", () => {
//    document.getElementById("joinCall").style.display = "block";
//});



connection.on("CallAccepted", () => {
    console.log("Call accepted");
    alert("Call accepted");
});

connection.on("CallDeclined", () => {
    console.log("Call declined");
    alert("Call declined");

});

