using Silk.NET.Maths;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private const int _speed = 128; // pixels per second
    private const int _maxHealth = 100;

    public int Health { get; private set; }
    public bool IsInvulnerable { get; private set; }
    private DateTimeOffset _lastHitTime;
    private const double _invulnerabilityDuration = 1.5; // seconds

    public enum PlayerStateDirection
    {
        None = 0,
        Down,
        Up,
        Left,
        Right,
    }

    public enum PlayerState
    {
        None = 0,
        Idle,
        Move,
        Attack,
        GameOver
    }

    public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SetState(PlayerState.Idle, PlayerStateDirection.Down);
        Health = _maxHealth;
        _lastHitTime = DateTimeOffset.MinValue;
    }

    public void SetState(PlayerState state)
    {
        SetState(state, State.Direction);
    }

    public void SetState(PlayerState state, PlayerStateDirection direction)
    {
        if (State.State == PlayerState.GameOver)
        {
            return;
        }

        if (State.State == state && State.Direction == direction)
        {
            return;
        }

        if (state == PlayerState.None && direction == PlayerStateDirection.None)
        {
            SpriteSheet.ActivateAnimation(null);
        }

        else if (state == PlayerState.GameOver)
        {
            SpriteSheet.ActivateAnimation(Enum.GetName(state));
        }
        else
        {
            var animationName = Enum.GetName(state) + Enum.GetName(direction);
            SpriteSheet.ActivateAnimation(animationName);
        }

        State = (state, direction);
    }

    public void GameOver()
    {
        SetState(PlayerState.GameOver, PlayerStateDirection.None);
    }

    public void Attack()
    {
        if (State.State == PlayerState.GameOver)
        {
            return;
        }

        var direction = State.Direction;
        SetState(PlayerState.Attack, direction);
    }

    public void TakeDamage(int amount)
    {
        if (State.State == PlayerState.GameOver || IsInvulnerable)
        {
            return;
        }

        Health = Math.Max(0, Health - amount);
        IsInvulnerable = true;
        _lastHitTime = DateTimeOffset.Now;

        if (Health <= 0)
        {
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (State.State != PlayerState.GameOver)
        {
            Health = Math.Min(_maxHealth, Health + amount);
        }
    }

    public void UpdateInvulnerability()
    {
        if (IsInvulnerable && (DateTimeOffset.Now - _lastHitTime).TotalSeconds >= _invulnerabilityDuration)
        {
            IsInvulnerable = false;
        }
    }

    public void UpdatePosition(double up, double down, double left, double right, int width, int height, double time)
    {
        UpdateInvulnerability();

        if (State.State == PlayerState.GameOver)
        {
            return;
        }

        var pixelsToMove = _speed * (time / 1000.0);

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y + (int)(down * pixelsToMove);
        y -= (int)(up * pixelsToMove);

        var newState = State.State;
        var newDirection = State.Direction;

        if (x == Position.X && y == Position.Y)
        {
            if (State.State == PlayerState.Attack)
            {
                if (SpriteSheet.AnimationFinished)
                {
                    newState = PlayerState.Idle;
                }
            }
            else
            {
                newState = PlayerState.Idle;
            }
        }
        else
        {
            newState = PlayerState.Move;
            
            if (y < Position.Y && newDirection != PlayerStateDirection.Up)
            {
                newDirection = PlayerStateDirection.Up;
            }

            if (y > Position.Y && newDirection != PlayerStateDirection.Down)
            {
                newDirection = PlayerStateDirection.Down;
            }

            if (x < Position.X && newDirection != PlayerStateDirection.Left)
            {
                newDirection = PlayerStateDirection.Left;
            }

            if (x > Position.X && newDirection != PlayerStateDirection.Right)
            {
                newDirection = PlayerStateDirection.Right;
            }
        }

        if (newState != State.State || newDirection != State.Direction)
        {
            SetState(newState, newDirection);
        }

        Position = (x, y);
    }
}