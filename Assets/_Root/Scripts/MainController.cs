using Ui;
using Game;
using Profile;
using UnityEngine;
using Features.Shed;
using Features.Inventory;
using Features.Shed.Upgrade;
using Tool;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

internal class MainController : BaseController
{
    private readonly Transform _placeForUi;
    private readonly ProfilePlayer _profilePlayer;
    private readonly List<GameObject> _subObjects = new List<GameObject>();
    private readonly List<IDisposable> _subDisposables = new List<IDisposable>();

    private MainMenuController _mainMenuController;
    private SettingsMenuController _settingsMenuController;
    private ShedController _shedController;
    private GameController _gameController;


    public MainController(Transform placeForUi, ProfilePlayer profilePlayer)
    {
        _placeForUi = placeForUi;
        _profilePlayer = profilePlayer;

        profilePlayer.CurrentState.SubscribeOnChange(OnChangeGameState);
        OnChangeGameState(_profilePlayer.CurrentState.Value);
    }

    protected override void OnDispose()
    {
        DisposeControllers();
        _profilePlayer.CurrentState.UnSubscribeOnChange(OnChangeGameState);
    }

    private void DisposeSubInstances()
    {
        DisposeSubDisposables();
        DisposeSubObjects();
    }

    private void DisposeSubDisposables()
    {
        foreach (IDisposable disposable in _subDisposables)
            disposable.Dispose();
        _subDisposables.Clear();
    }

    private void DisposeSubObjects()
    {
        foreach (GameObject gameObject in _subObjects)
            Object.Destroy(gameObject);
        _subObjects.Clear();
    }



    private ShedController CreateShedController(ProfilePlayer profilePlayer, Transform placeForUi)
    {
        InventoryContext inventoryContext = CreateInventoryContext(placeForUi, profilePlayer.Inventory);
        UpgradeHandlersRepository shedRepository = CreateShedRepository();
        ShedView shedView = LoadShedView(placeForUi);

        return new ShedController
        (
            shedView,
            profilePlayer,
            shedRepository
         );
    }

    private InventoryContext CreateInventoryContext(Transform placeForUi, InventoryModel model)
    {
        var context = new InventoryContext(placeForUi, model);
        _subDisposables.Add(context);

        return context;
    }

    private UpgradeHandlersRepository CreateShedRepository()
    {
        var path = new ResourcePath("Configs/Shed/UpgradeItemConfigDataSource");
        UpgradeItemConfig[] upgradeConfigs = ContentDataSourceLoader.LoadUpgradeItemConfigs(path);
        var repository = new UpgradeHandlersRepository(upgradeConfigs);
        _subDisposables.Add(repository);

        return repository;
        
    }

    private ShedView LoadShedView(Transform placeForUi)
    {
        var path = new ResourcePath("Prefabs/Shed/ShedView");
        GameObject pefab = ResourcesLoader.LoadPrefab(path);
        GameObject objectView = Object.Instantiate(pefab, _placeForUi, false);
        _subObjects.Add(objectView);

        return objectView.GetComponent<ShedView>();
    }


    private void OnChangeGameState(GameState state)
    {
        DisposeControllers();
        DisposeSubInstances();

        switch (state)
        {
            case GameState.Start:
                _mainMenuController = new MainMenuController(_placeForUi, _profilePlayer);
                break;
            case GameState.Settings:
                _settingsMenuController = new SettingsMenuController(_placeForUi, _profilePlayer);
                break;
            case GameState.Shed:
                _shedController = CreateShedController(_profilePlayer, _placeForUi);
                break;
            case GameState.Game:
                _gameController = new GameController(_placeForUi, _profilePlayer);
                break;
        }
    }

    private void DisposeControllers()
    {
        _mainMenuController?.Dispose();
        _settingsMenuController?.Dispose();
        _shedController?.Dispose();
        _gameController?.Dispose();
    }
}
