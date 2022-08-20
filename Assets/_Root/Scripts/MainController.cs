using Ui;
using Game;
using Profile;
using UnityEngine;
using Features.Shed;
using Features.Inventory;
using Features.Shed.Upgrade;
using Tool;

internal class MainController : BaseController
{
    private readonly Transform _placeForUi;
    private readonly ProfilePlayer _profilePlayer;
    private readonly ResourcePath _viewPath = new ResourcePath("Prefabs/Shed/ShedView");
    private readonly ResourcePath _dataSourcePath = new ResourcePath("Configs/Shed/UpgradeItemConfigDataSource");

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


    private ShedController CreateShedController(Transform placeForUi, ProfilePlayer profilePlayer)
    { 
    var _inventoryContext = CreateInventoryContext(placeForUi, _profilePlayer.Inventory);
    var _upgradeHandlersRepository = CreateRepository();
    var _view = LoadView(placeForUi);

        return new ShedController(_view, profilePlayer, _upgradeHandlersRepository);
    }


private InventoryContext CreateInventoryContext(Transform placeForUi, IInventoryModel model)
{
    var context = new InventoryContext(placeForUi, model);
    AddContext(context);

    return context;
}

private UpgradeHandlersRepository CreateRepository()
{
    UpgradeItemConfig[] upgradeConfigs = ContentDataSourceLoader.LoadUpgradeItemConfigs(_dataSourcePath);
    var repository = new UpgradeHandlersRepository(upgradeConfigs);
    AddRepository(repository);

    return repository;
}

private ShedView LoadView(Transform placeForUi)
{
    GameObject prefab = ResourcesLoader.LoadPrefab(_viewPath);
    GameObject objectView = Object.Instantiate(prefab, placeForUi, false);
    AddGameObject(objectView);

    return objectView.GetComponent<ShedView>();
}

private void OnChangeGameState(GameState state)
    {
        DisposeControllers();

        switch (state)
        {
            case GameState.Start:
                _mainMenuController = new MainMenuController(_placeForUi, _profilePlayer);
                break;
            case GameState.Settings:
                _settingsMenuController = new SettingsMenuController(_placeForUi, _profilePlayer);
                break;
            case GameState.Shed:
                _shedController = CreateShedController(_placeForUi, _profilePlayer);
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
        Debug.Log("1");
    }
}
