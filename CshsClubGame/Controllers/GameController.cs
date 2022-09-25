using CshsClubGame.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CshsClubGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameManager _gameManager;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger, GameManager gameManager)
        {
            _logger = logger;
            _gameManager = gameManager;
        }

#if DEBUG
        [HttpPost("Debug")]
        public ActionResult Debug(int d)
        {
            var room = _gameManager.CreateDebugRoom();
            for (int i = 0; i < d; i++)
            {
                var dummyPlayer = _gameManager.CreateDebugPlayer(i);
                _gameManager.JoinRoom(GameManager.LOBBY_ID, dummyPlayer);
            }
            return Ok("");
        }
#endif

        [HttpPost("CreateRoom")]
        public GameRoom CreateRoom(string roomName, int maxPlayers)
        {
            var room = _gameManager.CreateRoom(roomName, maxPlayers);
            return room;
        }

        [HttpPost("CreatePlayer")]
        public Player CreatePlayer(string classUnit, string name)
        {
            var player = _gameManager.CreatePlayer(classUnit, name);
            return player;
        }

        [HttpPost("JoinRoom")]
        public ActionResult JoinRoom([FromHeader] string selfId, string roomId)
        {
            var player = _gameManager.GetPlayerById(selfId);
            if (player == null)
            {
                return BadRequest("無效的玩家 Player ID");
            }

            var room = _gameManager.JoinRoom(roomId, player);
            if(room == null)
            {
                return BadRequest("加入房間失敗");
            }

            return Ok(room);
        }


        [HttpPost("GetCards")]
        public ActionResult GetCards([FromHeader] string selfId)
        {
            var player = _gameManager.GetPlayerById(selfId);
            if (player == null)
            {
                return BadRequest("無效的玩家 ID");
            }

            var cards = _gameManager.GetTurnCard(selfId);
            Console.WriteLine("cards.Count" + cards.Count);
            // 先寫效能差的髒扣，以後再改
            var result = new List<object>();
            foreach (var card in cards)
            {
                if (card.CardType == CardType.Character)
                {
                    Console.WriteLine(card.CardType);
                    result.Add((CharaterCard)card);
                }
                else if (card.CardType == CardType.Equipment)
                {
                    Console.WriteLine(card.CardType);
                    result.Add((EquipmentCard)card);
                }
                else if (card.CardType == CardType.Event)
                {
                    Console.WriteLine(card.CardType);
                    result.Add((EventCard)card);
                }
            }
            Console.WriteLine("result.Count" + result.Count);
            return Ok(result);
        }

        [HttpPost("SelectCard")]
        public TurnRecord SelectCard([FromForm] SelectCardRequest request)
        {
            var card = JsonObject.Parse(request.CardJson)!.AsObject();
            var record = _gameManager.ProcessTurnCard(request.SelfId, card);
            return record;
        }

        [HttpGet("PullHistory")]
        public GameHistoryEntry[] GetGameHistoryByNow()
        {
            return _gameManager.GetHistoryPage();
        }

        [HttpGet("Reset/{token}")]
        public ActionResult Reset(string token)
        {
            if (token == "12345")
            {
                _gameManager.Rest();
            }
            return Ok();
        }

        public class SelectCardRequest
        {
            public string SelfId { get; set; }
            public string CardJson { get; set; }
        }
    }
}