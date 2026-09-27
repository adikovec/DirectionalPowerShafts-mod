using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BlockingSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using Timberborn.StatusSystem;
using Timberborn.WorldPersistence;
using UnityEngine;

namespace DirectionalPowerShafts;

/// <summary>
/// A shaft may hold one outgoing dependency. The shaft in front waits until
/// this shaft has finished, just as a terrain block does.
/// </summary>
public sealed class DirectionalShaft : BaseComponent, IAwakableComponent,
    IInitializableEntity, IUnfinishedStateListener, IPersistentEntity,
    IPrePlacementChangeListener, IPostPlacementChangeListener
{
    private static readonly ComponentKey PersistenceKey = new("DirectionalPowerShafts");
    private static readonly PropertyKey<bool> EnabledKey = new("Enabled");

    private readonly IBlockService _blockService;
    private readonly ILoc _loc;
    private readonly EventBus _eventBus;
    private BlockObject _blockObject;
    private BlockableObject _blockableObject;
    private StatusToggle _waitingStatus;
    private DirectionalShaft _blockedBy;
    private DirectionalShaft _blocking;
    private GameObject _arrow;
    private bool _enabled;
    private bool _isUnfinished;

    public DirectionalShaft(IBlockService blockService, ILoc loc, EventBus eventBus)
    {
        _blockService = blockService;
        _loc = loc;
        _eventBus = eventBus;
    }

    public void Awake()
    {
        _blockObject = GetComponent<BlockObject>();
        _blockableObject = GetComponent<BlockableObject>();
        _waitingStatus = StatusToggle.CreateNormalStatus(
            "DirectionalBlocking",
            _loc.T("Status.Buildings.DirectionalBlocking"));
    }

    public void InitializeEntity()
    {
        GetComponent<StatusSubject>().RegisterStatus(_waitingStatus);
    }

    public void OnEnterUnfinishedState()
    {
        _isUnfinished = true;
        _eventBus.Register(this);
        RefreshArrow();
        LinkNeighbours();
    }

    public void OnExitUnfinishedState()
    {
        _eventBus.Unregister(this);
        Disconnect();
        _isUnfinished = false;
        RefreshArrow();
    }

    public void OnPrePlacementChanged()
    {
        Disconnect();
    }

    public void OnPostPlacementChanged()
    {
        RefreshArrow();
        LinkNeighbours();
    }

    [OnEvent]
    public void OnNeighbourEnteredUnfinishedState(EnteredUnfinishedStateEvent eventData)
    {
        var other = eventData.BlockObject;
        var delta = other.Coordinates - _blockObject.Coordinates;
        if (delta.z == 0 && Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1
            && other.GetComponent<DirectionalShaft>() != null)
        {
            LinkNeighbours();
        }
    }

    public void EnableForNewSite()
    {
        if (_blockObject.IsFinished)
        {
            return;
        }
        _enabled = true;
        RefreshArrow();
        LinkNeighbours();
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (_enabled)
        {
            entitySaver.GetComponent(PersistenceKey).Set(EnabledKey, value: true);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        _enabled = entityLoader.TryGetComponent(PersistenceKey, out var component)
            && component.Has(EnabledKey) && component.Get(EnabledKey);
    }

    private Vector3Int Forward => _blockObject.TransformCoordinates(-Vector3Int.up);
    private Vector3Int Behind => _blockObject.TransformCoordinates(Vector3Int.up);

    private void LinkNeighbours()
    {
        if (!_enabled || !_isUnfinished)
        {
            return;
        }

        var behind = _blockService.GetObjectsWithComponentAt<DirectionalShaft>(Behind)
            .FirstOrDefault(candidate => candidate._enabled && candidate._isUnfinished
                && candidate.Forward == _blockObject.Coordinates);
        if (behind != null)
        {
            Link(behind, this);
        }

        var ahead = _blockService.GetObjectsWithComponentAt<DirectionalShaft>(Forward)
            .FirstOrDefault(candidate => candidate._enabled && candidate._isUnfinished
                && candidate.Behind == _blockObject.Coordinates);
        if (ahead != null)
        {
            Link(this, ahead);
        }
    }

    private static void Link(DirectionalShaft behind, DirectionalShaft ahead)
    {
        if (ahead._blockedBy == behind)
        {
            return;
        }
        ahead.DisconnectFromBehind();
        behind.DisconnectFromAhead();
        ahead._blockedBy = behind;
        behind._blocking = ahead;
        ahead._blockableObject.Block(behind);
        ahead._waitingStatus.Activate();
    }

    private void Disconnect()
    {
        DisconnectFromBehind();
        DisconnectFromAhead();
    }

    private void DisconnectFromBehind()
    {
        if (_blockedBy == null)
        {
            return;
        }
        _blockableObject.Unblock(_blockedBy);
        _blockedBy._blocking = null;
        _blockedBy = null;
        _waitingStatus.Deactivate();
    }

    private void DisconnectFromAhead()
    {
        if (_blocking == null)
        {
            return;
        }
        _blocking.DisconnectFromBehind();
    }

    private void RefreshArrow()
    {
        if (_arrow == null)
        {
            _arrow = ShaftArrow.Create(GameObject.transform);
        }
        // Preview and construction-site roots can have different parent
        // rotations. Keep the arrow tied to the grid placement orientation.
        _arrow.transform.rotation = _blockObject.Orientation.ToWorldSpaceRotation();
        _arrow.SetActive(_blockObject.IsPreview || (_enabled && _isUnfinished));
    }
}
