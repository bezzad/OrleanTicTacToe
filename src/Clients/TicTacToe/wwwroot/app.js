const playerIdKey = 'x-player-id';
var oxo = {
    rand: function () {
        return ""; // "?r=" + Math.random();
    },

    user: {
        username: null,
        id: null,
        email: null,
        wins: null,
        loses: null
    },

    model: {
        currentGameId: null,
        currentGames: [],
        availableGames: []
    },

    ajax: {
        createGame: function (cb) {
            $.post("/Game/CreateGame", function (data) {
                if (data) {
                    data.waiting = data.state == 0;
                    oxo.model.currentGames.push(data);
                }
                cb(oxo.model);
            });
        },
        getGames: function (cb) {
            $.get("/Game" + oxo.rand(), function (data) {
                if (data) {
                    // games playing
                    data.currentGames.forEach(function (x) {
                        x.waiting = x.state == 0;
                    });
                    oxo.model.currentGames = data.currentGames;
                    oxo.model.availableGames = data.availableGames;
                }
                cb(oxo.model);
            });
        },
        getMoves: function (cb) {
            $.get("/Game/Moves/" + oxo.model.currentGameId + oxo.rand(), cb);
        },
        makeMove: function (x, y, cb) {
            $.post("/Game/Move/" + oxo.model.currentGameId + "/?x=" + x + "&y=" + y, cb);
        },
        joinGame: function (gameId, cb) {
            $.post("/Game/Join/" + gameId, function (data) {
                // check we have joined
                oxo.model.currentGameId = gameId;
                cb(data);
            });
        },
        setName: function (name, cb) {
            $.post("/Player/SetUsername/" + name + oxo.rand(), function (data) {
                updateUserModel(data);
                if (cb) {
                    cb(data);
                }
            });
        },
        getUser: function (cb) {
            $.get("/Player/Info", function (data) {
                if (cb) {
                    cb(data);
                }
            });
        }
    },

    controllers: {
        refreshHeader: function () {
            oxo.ajax.getUser(user => {
                updateUserModel(user);
                if (!oxo.user.username) {
                    $("#enter-name-modal").modal({
                        backdrop: 'static',
                        keyboard: false
                    });
                }
                oxo.ui.renderHeader(oxo.user);
            });
        },
        refreshGamesList: function () {
            oxo.ajax.getGames(oxo.ui.renderGameList);
        },
        refreshBoard: function () {
            if (oxo.model.currentGameId) {
                oxo.ajax.getMoves(function (data) {
                    oxo.ui.renderBoard(data);
                });
            }
        },
        changeName: function () {
            $("#enter-name-modal").modal('show');
        },
        play: function (gameId) {
            oxo.model.currentGameId = gameId;
            oxo.controllers.refreshBoard();
            $("#board-placeholder").show("fast");
            $("#games-placeholder").hide("fast");
        },
        move: function (x, y) {
            oxo.ajax.makeMove(x, y, function (data) {
                if (data) {
                    oxo.ui.renderBoard(data);
                }
            });
        },
        createGame: function () {
            oxo.ajax.createGame(oxo.ui.renderGameList);
        },
        showJoinDialog: function () {
            $("#join-game-modal").modal();
        },
        joinGame: function () {
            var gameId = $("#join-game-input").val().trim();
            if (!gameId) return;
            $("#join-game-modal").modal('hide');
            oxo.ajax.joinGame(gameId, function (data) {
                $("#join-game-input").val("");
                oxo.controllers.refreshGamesList();
            });
        },
        joinThisGame: function (gameId) {
            if (!gameId) return;
            oxo.ajax.joinGame(gameId, function (data) {
                oxo.controllers.refreshGamesList();
            });
        },
        enterName: function () {
            var name = $("#enter-name-input").val().trim();
            if (!name) return;
            oxo.user.username = name;
            $("#enter-name-modal").modal('hide');
            oxo.ajax.setName(name, function () {
                $("#enter-name-input").val("")
                oxo.controllers.refreshHeader();
                oxo.controllers.refreshGamesList();
            });
        },
        showInvite: function (gameId) {
            $("#invite-game-id").val(gameId);
            $("#invite-game-link").val(window.location.origin + "/Game/Join/" + gameId);
            $("#invite-game-modal").modal();
        },
        showGames: function () {
            $("#board-placeholder").hide("fast");
            $("#games-placeholder").show("fast");
        }
    },

    ui: {
        renderHeader: function (data) {
            var template = Handlebars.compile($("#header-template").html());
            $("#header-placeholder").html(template(data));
        },
        renderGameList: function (data) {
            var template = Handlebars.compile($("#games-template").html());
            $("#games-placeholder").html(template(data));
        },
        renderBoard: function (data) {
            var template = Handlebars.compile($("#board-template").html());
            var board = {};
            if (data.summary.yourMove) {
                for (var x = 0; x < 3; x++)
                    for (var y = 0; y < 3; y++)
                        board["x" + x + "y" + y] = '<a href="javascript:void(0);" onclick="oxo.controllers.move(' + x + ', ' + y + ');">MOVE</a>'
            }
            var useO = true;
            data.moves.forEach(function (move) {
                board["x" + move.x + "y" + move.y] = useO ? "O" : "X";
                useO = !useO;
            });
            data.board = board;
            $("#board-placeholder").html(template(data));
        }
    }
}

function updateUserModel(data) {
    if (data.id) {
        oxo.user = data;
        setCookie(playerIdKey, data.id, 30);
    }
}

function setCookie(cname, cvalue, exdays) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

$(document).ready(function () {
    oxo.user.id = getCookie(playerIdKey);
    $.ajaxSetup({
        beforeSend: function (xhr) {
            xhr.setRequestHeader('x-appname', "tic tac toe pure js client");
            xhr.setRequestHeader(playerIdKey, oxo.user.id);
        },
        complete: function (xhr) {
            var id = xhr.getResponseHeader(playerIdKey);
            if (id) {
                oxo.user.id = id;
                setCookie(playerIdKey, id, 30);
            }
        }
    });

    oxo.controllers.refreshHeader();
    oxo.controllers.refreshGamesList();

    $("#joinConfirmButton").bind('click', oxo.controllers.joinGame);
    $("#enterNameOk").bind('click', oxo.controllers.enterName);
    $("#enter-name-input").keyup(function (event) {
        if (event.keyCode == 13) {
            $("#enterNameOk").click();
        }
    });


    var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();
    connection.on("OnNewGame", function (game) {
        if (game && game.ownerPlayerId != oxo.user.id) {
            // just add other players games
            oxo.model.availableGames.push(game);
            oxo.ui.renderGameList(oxo.model);
            console.log("New game signal received from Orlean, added to available games");
            console.log(game);
        }
    });

    connection.on("OnUpdateBoard", function (data) {
        if (data) {
            oxo.model.currentGameId = data.summary.gameId;
            let currentGameIndex = oxo.model.currentGames.findIndex((game, index, obj) => {
                return game.gameId == data.summary.gameId;
            })
            oxo.model.currentGames[currentGameIndex] = data.summary;
            oxo.ui.renderGameList(oxo.model);
            oxo.ui.renderBoard(data);
            console.log("board updated");
            console.log(data);
        }
    });

    connection.start()
        .then(function () {
            console.log("SignalR connected.");
            // TODO: authorize current user with signal to server.
            // keep pair connectionId with playerId
            // connection.invoke("signin", oxo.user);
        }).catch(err => {
            console.error(err.toString());
        });
});



