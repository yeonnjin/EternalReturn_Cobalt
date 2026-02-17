using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Net.WebSockets;
using System.Numerics;

public class Player_SkillState : IPlayerState, IReceivesMoveCommand, IReceivesStopCommand
{
    private readonly ISkill _handler;
    public ISkill Handler {  get { return _handler; } }
    public readonly SkillContext Ctx;

    private long _tStartTick, _tEndTick;
    private bool _forceEnd;

    private Vector3? _currentDestination = null;
    private const float DEST_CHANGE_EPS = 0.05f; // 목적지 미세변경 무시

    public bool CanStopSkill => _handler.CanStopSkill;

    public Player_SkillState(ISkill handler, SkillContext ctx)
    {
        _handler = handler;
        Ctx = ctx;
        Ctx.AttachFinishHandler(RequestFinish);
    }

    public void Enter(Player player)
    {
        _tStartTick = TimeUtil.Instance.LastTick;

        float durSec = _handler.GetDuration();
        _tEndTick = unchecked(_tStartTick + (int)MathF.Round(durSec * 1000f));

        _handler.OnEnter(player, Ctx);
    }

    public void Execute(Player player)
    {
        if (_forceEnd || TimeUtil.Instance.IsPastOrNow(_tEndTick))
        {
            ChangeState(player);
            return;
        }

        if(HandleMovementCompletion(player))
            OnStopCommand(player, null);

        _handler.OnTick(player, Ctx);
    }

    public void Exit(Player player)
    {
        _handler.OnExit(player, Ctx);
    }
     
    private void ChangeState(Player player)
    {
        if (player.Intent.TryConsume(out var dest))
        {
            if (player.TryHandleMoveWithTokens(dest))
                return;

            C_Move cmd = new C_Move()
            {
                IsTargetOn = !dest.IsGround,
                TargetId = dest.TargetId,
                TargetPosition = dest.TargetPos,
            };

            player.ChangeState(new Player_MovingState(cmd));
            player.SendMoveSyncPacket(dest.TargetPos);
        }
        else
        {
            player.ChangeState(new Player_IdleState());
        }
    }

    // 스킬 중에 이동 시 목적지까지 도착했는지
    private bool HandleMovementCompletion(Player player)
    {
        if (_currentDestination.HasValue)
        {
            Vector3 playerPos = player.PosInfo.ToVector();

            float distanceSq = Vector3.DistanceSquared(playerPos, _currentDestination.Value);

            if (distanceSq < 0.05)
                return true;
        }
        return false;
    }

    public void OnMoveCommand(Player player, C_Move move)
    {
        if (_handler.CanMoveDuringCast)
        {
            // (A) 시전 중 이동 허용
            _currentDestination = move.TargetPosition.ToVector();
            _handler.OnMove(player, move);
        }
        else
        {
            if (_handler.CanStopSkill)
            {
                player.ChangeState(new Player_MovingState(move));
                player.SendMoveSyncPacket(move.TargetPosition);
                player.Intent.Clear();
            }
            else
            {
                // (B) 시전 중 이동 불가 스킬
                // 나중에 스킬이 끝나면 바로 이동시키기 위해 의도를 큐에 넣음
                C_SetMoveTarget deferred = new C_SetMoveTarget()
                {
                    IsGround = !move.IsTargetOn,
                    TargetId = move.TargetId,
                    TargetPos = move.TargetPosition
                };
                player.EnqueueMove(deferred);
            }   
        }
    }

    #nullable enable
    public void OnStopCommand(Player player, C_Stop? stopPacket)
    {
        _currentDestination = null;
        _handler.OnStop(player);
    }
    #nullable disable

    public void RequestFinish(SkillFinishReason reason = SkillFinishReason.EarlyEnd)
    {
        _forceEnd = true;
    }
}

