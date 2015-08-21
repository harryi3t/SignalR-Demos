var player = {
    "Id": "",
    "Name": "",
};
var opponent = {
    "Group": "",
    "Hash": "",
    "Id": "",
    "Name": "",
};

var game = { Pieces: new Array() };
game.Pieces[0] = { Name: 'card-1' };
game.Pieces[1] = { Name: 'card-2' };
game.Pieces[2] = { Name: 'card-3' };
game.Pieces[3] = { Name: 'card-4' };
game.Pieces[4] = { Name: 'card-5' };

game.Pieces[5] = { Name: 'card-6' };
game.Pieces[6] = { Name: 'card-7' };
game.Pieces[7] = { Name: 'card-8' };
game.Pieces[8] = { Name: 'card-9' };
game.Pieces[9] = { Name: 'card-10' };

game.Pieces[10] = { Name: 'card-11' };
game.Pieces[11] = { Name: 'card-12' };
game.Pieces[12] = { Name: 'card-13' };
game.Pieces[13] = { Name: 'card-14' };
game.Pieces[14] = { Name: 'card-15' };


var app = angular.module("game", []);
app.controller("boardController", function ($scope) {
    $scope.size = 5;
    $scope.game = [];
    $scope.loadBoard = function () { $scope.game = game;};
});

$(function () {
    $("#join").attr("disabled", "disabled");

    var hub = $.connection.gameHub;
    $.connection.hub.start().done(function () {
        $("#join").removeAttr("disabled");
    });

    $("#join").click(function () {
        var un = $("#username").val();
        hub.server.join(un);
    });

 
    hub.client.playerJoined = function (user) {
        player.Id = user.Id;
        player.Name = user.Name;
        $("#name").append(user.Name);
        $("#username").attr("disabled", "disabled");
        $("#join").attr("disabled", "disabled");
    };

    hub.client.waitingList = function () {
        $("#alert").html("At this time there is not an opponent. "
            +"As soon as one joins, your game will be started.");
    }; 

    hub.client.playerExists = function (user) {
        $("#alert").html("Player " + $("#username").val() + " already exists. Reloading Board");
        player.Id = user.Id;
        player.Name = user.Name;
        $("#name").append(user.Name);
        $("#username").attr("disabled", "disabled");
        $("#join").attr("disabled", "disabled");
    };

    hub.client.buildBoard = function (g) {
        console.log(g);
        game.Pieces = g.Board.Pieces;
        $("#loadButton").click();
        if(g.Player1.Id == player.Id)
            opponent = g.Player2;
        else
            opponent = g.Player1;

        if(g.WhosTurn == player.Id)
            $("#alert").html("Game Started with " + opponent.Name + "</br>You get to Flip first!");
        else
            $("#alert").html("Game Started with " + opponent.Name + "</br>Your opponent get to Flip first!");

        $("div[id^=card-]").click(function (e) {
            e.preventDefault();
            var id = this.id;
            var card = $("#" + id);

            if (card.hasClass("match") || card.hasClass("flip"))
                return;
            hub.server.flip(id)
            .done(function (result) {
                if (result)
                    hub.server.checkCard(id);
            });
            
        });
    };

    hub.client.flipCard = function (card) {
        $("#card-" + card.Id).addClass("flip");
    }

    hub.client.resetFlip = function (cardA, cardB) {
        console.log(cardA);
        console.log(cardB);
        var cA = $("#card-" + cardA.Id);
        var cB = $("#card-" + cardB.Id);
        var delay = setTimeout(function () {
            cA.removeClass("flip");
            cB.removeClass("flip");
        }, 1500);
    }

    hub.client.showMatch = function (card, userName) {
        $("#card-" + card.Id).addClass("match");
        $("#card-" + card.Pair).addClass("match");
        $("#alert").html("<strong>" + userName + "</strong> found a match!");
        if (player.Name = userName)
            $("#wins").append("<li>").append("<img height='30' src='"+card.Image+"'/>");
    }

    $("#dialog-message").dialog({
        autoOpen:false,
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

    hub.client.winner = function (card, userName) {
        $("#card-" + card.Id).addClass("match");
        $("#card-" + card.Pair).addClass("match");
        $("#alert").html("<strong>" + userName + "</strong> found a match!");
        if (player.Name = userName)
            $("#wins").append("<li>").append("<img height='30' src='" + card.Image + "'/>");
        
        if(player.Name == userName)
            $("#modal-message").html("<strong>Contratulations!</strong> You Won");
        else
            $("#modal-message").html("<strong>Sorry!</strong> You Lost");
        $("#dialog-message").dialog().dialog("open");
    }
});


