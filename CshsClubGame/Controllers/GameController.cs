using CshsClubGame.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CshsClubGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
                _gameManager.JoinRoom(room.Id, dummyPlayer);
            }
            return Ok();
        }
#endif

        [HttpPost("CreateRoom")]
        public GameRoom CreateRoom(string roomName, int maxPlayers)
        {
            var room = _gameManager.CreateRoom(roomName, maxPlayers);
            return room;
        }

        [HttpPost("CreatePlayer")]
        public Player CreatePlayer(string classUnit, string seatNo, string name)
        {
            var player = _gameManager.CreatePlayer(classUnit, seatNo, name);
            return player;
        }

        [HttpPost("JoinRoom")]
        public ActionResult JoinRoom([FromHeader] string selfId, string roomId)
        {
            var player = _gameManager.GetPlayer(selfId);
            if (player == null)
            {
                return BadRequest("�L�Ī� Player ID");
            }

            var room = _gameManager.JoinRoom(roomId, player);
            if(room == null)
            {
                return BadRequest("�[�J�ж�����");
            }
            return Ok(room);
        }


        [HttpPost("GetCards")]
        public ActionResult GetCards([FromHeader] string selfId)
        {
            var player = _gameManager.GetPlayer(selfId);
            if (player == null)
            {
                return BadRequest("�L�Ī����a ID");
            }

            var cards = _gameManager.GetTurnCards(selfId);
            // ���g�į�t��ż���A�H��A��
            var result = new List<object>();
            foreach (var card in cards)
            {
                if (card.CardType == "����")
                    result.Add((CharaterCard)card);
                else if (card.CardType == "�˳�")
                    result.Add((EquipmentCard)card);
                else if (card.CardType == "�ƥ�")
                    result.Add((EventCard)card);
            }
            return Ok(result);
        }

        [HttpPost("SelectCard")]
        public ActionResult SelectCard([FromHeader] string selfId, JsonObject card)
        {
            var record = _gameManager.ProcessTurnCard(selfId, card);
            if (record != null)
            {
                return Ok(card);
            }
            return BadRequest("���w�q���d������");
        }
    }
}