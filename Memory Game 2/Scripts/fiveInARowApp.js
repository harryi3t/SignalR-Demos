var player = {
    "Id": "",
    "Name": "",
    "Symbol": ""
};
var opponent = {
    "Group": "",
    "Hash": "",
    "Id": "",
    "Name": "",
};

var isRefreshed = false;

var game = { Rows: new Array() };
//game.Rows[0] = [1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[1] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[2] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[3] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[4] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[5] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[6] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[7] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[8] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[9] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[10] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[11] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//game.Rows[12] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

var obj;

var app = angular.module("game", []);
app.controller("boardController", function ($scope) {
    $scope.size = 5;
    $scope.game = [];
    $scope.loadBoard = function () { $scope.game = game; };
    $scope.resetBoard = function () { $scope.game = []; };
    $scope.returnClass = function (x, y) {
        if ($scope.game.Rows[x][y] == 0)
            return "tile";
        else if ($scope.game.Rows[x][y] == 1)
            return "tile RedSymbol";
        else if ($scope.game.Rows[x][y] == 2)
            return "tile BlueSymbol";
    };
});

$(function () {
    $("#join").attr("disabled", "disabled");

    var hub = $.connection.fiveInARowHub;
    //$.connection.hub.logging = true;
    
    $("#join").click(function () {
        var un = $("#username").val();
        hub.server.join(un);
    });

    hub.client.hello = function (mesg) {
        console.log(mesg);
    }

    hub.client.playerJoined = function (user) {
        player = user;
        console.log(user);
        $("#name").html("<h2>User: " + user.Name + "</h2>");
        $("#username").attr("disabled", "disabled");
        $("#join").attr("disabled", "disabled");
    };

    $.connection.hub.start().done(function () {
        $("#join").removeAttr("disabled");
    });

    hub.client.waitingList = function () {
        $("#alert").html("At this time there is not an opponent. "
            + "As soon as one joins, your game will be started.");
    };

    hub.client.playerExists = function (user) {
        $("#alert").html("Player " + $("#username").val() + " already exists. Reloading Board");
        player = user;
        $("#name").html("<h2>User: "+user.Name+"</h2>");
        $("#username").attr("disabled", "disabled");
        $("#join").attr("disabled", "disabled");
        isRefreshed = true;
    };

    hub.client.buildBoard = function (g) {
        game.Rows = g.Board.board;

        $("#loadButton").click();
        if (g.Player1.Id == player.Id) {
            opponent = g.Player2;
            player = g.Player1;
        }
        else {
            opponent = g.Player1;
            player = g.Player2;
        }

        if (g.Player1.Symbol == "RedSymbol") {
            $("#redName").html(g.Player1.Name);
            $("#blueName").html(g.Player2.Name);
        }
        else {
            $("#redName").html(g.Player2.Name);
            $("#blueName").html(g.Player1.Name);
        }


        if (g.WhosTurn == player.Id)
            $("#alert").html("Game Started with " + opponent.Name + "</br>You get to first first!");
        else
            $("#alert").html("Game Started with " + opponent.Name + "</br>Your opponent gets to play first!");

        if (isRefreshed) {
            $("#alert").html("Board Reloaded!");
            $("#tile-" + g.lastX + "-" + g.lastY).addClass("lastMarked");
        }
        if (g.WhosTurn == player.Id)
            $("#alert").append("</br>It's your turn now.");
        else
            $("#alert").append("</br>It's your opponent turn now.");

        $("div[id^=tile-]").click(function (e) {
            e.preventDefault();
            var id = this.id.substr(5);
            var x = id.split("-")[0] % 13;
            var y = id.split("-")[1] % 13;
            hub.server.checkMark(x, y).done(function (result) {
                if (!result) console.log("Game not initialised");
            });
        });
    };

    hub.client.playNextMove = function (user, x, y) {
        clearTimeout(timeOut);

        $(".lastMarked").removeClass("lastMarked");
        $("#tile-" + x + "-" + y).addClass(user.Symbol).addClass("lastMarked");
        if (user.Symbol == "RedSymbol") {
            $("#blueProgressBar").progressbar("value", 100);
            blueProgress();
        }
        else if (user.Symbol == "BlueSymbol") {
            $("#redProgressBar").progressbar("value", 100);
            redProgress();
        }
        else throw ("Error symbol not found.");
    };

    $("#dialog-message").dialog({
        autoOpen: false,
        modal: true,
        buttons: {
            Close: function () {
                $(this).dialog("close");
                $("#join").removeAttr("disabled");
                $("#username").removeAttr("disabled");
                $("#wins").html();
                $("#alert").html("Please enter your name and click <strong>Join</strong> to play.");
                $("#board").html();
            }
        }
    });

    hub.client.winner = function (userName) {
       console.log(userName);
       if (player.Name == userName)
            $("#modal-message").html("<strong>Contratulations!</strong> You Won");
        else
            $("#modal-message").html("<strong>Sorry!</strong> You Lost");
       $("#dialog-message").dialog().dialog("open");

       $("#join").removeAttr("disabled");      
       $("#username").val("")
       $("#alert").html("Game Over. Type your username and click <strong>Join</strong> to play again");
       $("#name").html("<h2>User: </h2>");
       $("#resetButton").click();
    }

    $("#redProgressBar").progressbar({
        value: 100
    });

    $("#blueProgressBar").progressbar({
        value: 100
    });

    var timeOut;

    function redProgress() {
        var val = $("#redProgressBar").progressbar("value") || 0;

        $("#redProgressBar").progressbar("value", val - 1);

        if (val > 0) {
            timeOut = setTimeout(redProgress, 200);
        }
        else {
            if (player.Symbol == "RedSymbol") // Only opponent can trigger
                hub.server.timeOut();
            $("#blueProgressBar").progressbar("value", 100);
            blueProgress();
        }
    }

    function blueProgress() {
        var val = $("#blueProgressBar").progressbar("value") || 0;

        $("#blueProgressBar").progressbar("value", val - 1);

        if (val > 0) {
            timeOut = setTimeout(blueProgress, 200);
        }
        else {
            if (player.Symbol == "RedSymbol") // Only opponent can trigger
                hub.server.timeOut();
            $("#redProgressBar").progressbar("value", 100);
            redProgress();
        }
    }
});


