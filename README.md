# Unity uGui bindings for LeoECS Lite
Bindings for events from Unity uGui to [LeoECS Lite](https://github.com/Leopotam/ecslite).

> C#7.3 or above required for this framework.

> Tested on unity 2020.3 (dependent on it) and contains assembly definition for compiling to separate assembly file for performance reason.

> Dependent on [LeoECS Lite](https://github.com/Leopotam/ecslite) - dependency should be imported to unity project first.
> Dependent on [LeoECS Lite Extended systems](https://github.com/Leopotam/ecslite-extendedsystems) - dependency should be imported to unity project first.

# Table of content
* [Socials](#socials)
* [Installation](#installation)
    * [As unity module](#as-unity-module)
    * [As source](#as-source)
* [Integration](#integration)
    * [EcsUguiEmitter](#ecsuguiemitter)
    * [EcsUguiCallbackSystem](#ecsuguicallbacksystem)
    * [Actions](#actions)
    * [Components](#components)
* [License](#license)

# Socials
[![discord](https://img.shields.io/discord/404358247621853185.svg?label=enter%20to%20discord%20server&style=for-the-badge&logo=discord)](https://discord.gg/5GZVde6)

# Installation

## As unity module
This repository can be installed as unity module directly from git url. In this way new line should be added to `Packages/manifest.json`:
```
"com.leopotam.ecslite.unity.ugui": "https://github.com/Leopotam/ecslite-unity-ugui.git",
```
By default last released version will be used. If you need trunk / developing version then `develop` name of branch should be added after hash:
```
"com.leopotam.ecslite.unity.ugui": "https://github.com/Leopotam/ecslite-unity-ugui.git#develop",
```

## As source
If you can't / don't want to use unity modules, code can be cloned or downloaded as archive from `releases` page.

# Integration

## EcsUguiEmitter
Special MonoBehaviour that will convert ugui-events on scene to ecs-specific events. Should be placed on root GameObject of Ui hierarchy in scene (on root Canvas, for example) and connected in `ecs world` before any systems that should process events from ui:
```csharp
public class Startup : MonoBehaviour {
    // Field that should be initialized by instance of `EcsUguiEmitter` assigned to Ui root GameObject.
    [SerializeField] EcsUguiEmitter _uguiEmitter;

    EcsSystems _systems;

    void Start () {
        _systems = new EcsSystems (new EcsWorld ());
        _systems
            .Add (new Test1System ())
            .Add (new Test2System ())
            // better to create custom world for ugui events to reduce memory consuming for default world.
            .AddWorld (new EcsWorld (), "ugui-events")
            // should be placed after all systems with gui events dependency.
            .InjectUgui (_uguiEmitter, "ugui-events")
            .Init ();
    }
    
    void Update () {
        _systems?.Update ();
    }
    
    void OnDestroy () {
        if (_systems != null) {
            _systems.GetWorld ("ugui-events").Destroy ();
            _systems.GetWorld ().Destroy ();
            _systems = null;
        }
    }
}

public class Test1System : IEcsInitSystem {
    // ugui injected fields.
    readonly EcsUguiEmitter _ugui = default;
    
    GameObject _btnGo;
    Transform _btnTransform;
    Button _btn;

    public void Init (EcsSystems systems) {
        _btnGo = _ugui.GetNamedObject ("MyButton");
        _btnTransform = _ugui.GetNamedObject ("MyButton").GetComponent<Transform> ();
        _btn = _ugui.GetNamedObject ("MyButton").GetComponent<Button> ();
    }
}

public class Test2System : IEcsInitSystem {
    // ugui injected fields.
    [EcsUguiNamed("MyButton")] GameObject _btnGo;
    [EcsUguiNamed("MyButton")] Transform _btnTransform;
    [EcsUguiNamed("MyButton")] Button _btn;

    public void Init (EcsSystems systems) {
        // All fields above will be filled with same values as in Test1System.
    }
}
```

## EcsUguiCallbackSystem
Methods can be directly subscribed to ugui events through `EcsUguiCallbackSystem`:
```csharp
public class TestUguiClickEventSystem : EcsUguiCallbackSystem {
    [Preserve] // for fix possible il2cpp dead code elimination.
    [EcsUguiClickEvent]
    void OnAnyClick (in EcsUguiClickEvent evt) {
        Debug.Log ("Im clicked!", evt.Sender);
    }
    
    // method will be called only on "exit-button" widget click event. 
    [Preserve]
    [EcsUguiClickEvent("exit-button")]
    void OnExitButtonClicked (in EcsUguiClickEvent evt) {
        Debug.Log ("exit-button clicked!", evt.Sender);
    }
    
    // method will be called only on "exit-button" widget click event at "events" world. 
    [Preserve]
    [EcsUguiClickEvent("exit-button", "events")]
    void OnExitButtonClicked (in EcsUguiClickEvent evt) {
        Debug.Log ("exit-button clicked!", evt.Sender);
    }
}
```
Supported attributes:
```csharp
[EcsUguiClickEvent]
[EcsUguiUpEvent]
[EcsUguiDownEvent]
[EcsUguiDragStartEvent]
[EcsUguiDragMoveEvent]
[EcsUguiDragEndEvent]
[EcsUguiEnterEvent]
[EcsUguiExitEvent]
[EcsUguiScrollViewEvent]
[EcsUguiSliderChangeEvent]
[EcsUguiTmpDropdownChangeEvent]
[EcsUguiTmpInputChangeEvent]
[EcsUguiTmpInputEndEvent]
[EcsUguiDropEvent]
```
## Actions
MonoBehaviours that should be added to uGui widgets to transfer events from them to `ecs-world` (`EcsUguiClickAction`, `EcsUguiDragAction` and others). Each action component contains reference to `EcsUguiEmitter` in scene (if not inited manually - will try to find emitter automatically on start) and logical name `WidgetName` that can helps to detect source of event (or just get named `GameObject`) inside ecs-system.

## Components
Event data containers: `EcsUguiClickEvent`, `EcsUguiBeginDragEvent`, `EcsUguiEndDragEvent` and others - they can be used as ecs-components with standard filtering through `EcsFilter`:
```csharp
public class TestUguiClickEventSystem : IEcsInitSystem, IEcsRunSystem {
    EcsPool<EcsUguiClickEvent> _clickEventsPool;
    EcsFilter _clickEvents;
    
    public void Init (EcsSystems systems) {
        var world = systems.GetWorld ("ugui-events");
        _clickEventsPool = world.GetPool<EcsUguiClickEvent> (); 
        _clickEvents = world.Filter<EcsUguiClickEvent> ().End ();
    }

    public void Run (EcsSystems systems) {
        foreach (var entity in _clickEvents) {
            ref EcsUguiClickEvent data = ref _clickEventsPool.Get (entity);
            Debug.Log ("Im clicked!", data.Sender);
        }
    }
}
```

# License
The software is released under the terms of the [MIT license](./LICENSE.md).

No personal support or any guarantees.
