**Important! This repository is just a proof-of-concept, not suitable for the development of solutions.** 


*Anyway you can find here some useful implementations for features such as the OBJ to glTF 3D model converter, data serialization, UI toolbar, file browser, etc.*


---

# OpenVISDIVE
### Open source Visualization and Interaction with Scenario Data In Virtual Environments for Unity
**Requires Unity 2018.3 or higher (not yet tested with latest Unity versions).**

**This project includes [AsImpL](https://github.com/gpvigano/AsImpL) and a slightly modified version of [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) for loading 3D models at run-time.**


This project aims at providing a framework for visual interaction with scenario data in virtual environments.


Scenario data can span from the layout of a factory to the configuration of a product, the fitting-out of an apartment and so on.
The main goal is to provide an open framework, enabling the development of decision support systems for the interaction with scenario data in a Virtual Environment, where real objects are properly represented with their appearance and behavior. Data related to features, constraints, relationships and transformations are encoded in a proper visual and behavioral representation to make them better understandable. A suitable interface is provided to interact with such data in an intuitive way.

Data must be loaded and saved from a repository in a proper format to enable data exchange with external applications.


A scenario editor is developed along with the framework, in order to demonstrate how to interoperate with data coming from external sources (an XML data exchange module has been developed for testing and demonstration purposes, but other *connectors* could be implemented in future).


 ![image](https://raw.githubusercontent.com/gpvigano/OpenVISDIVE/master/images/Unity-OpenVISDIVE_Editor.png)

*Sample OpenVISDIVE Editor*

The user interface of this tool is based on [an ontology-based virtual factory tool](https://www.scopus.com/record/display.uri?eid=2-s2.0-85045567162&origin=resultslist), developed in C++ using OpenGL.
This version is based on [**VrSimTk**](https://github.com/gpvigano/VRSimTk) for project data management and simulation and uses [AsImpL](https://github.com/gpvigano/AsImpL) as loader for OBJ models, [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) for glTF assets.

When placing resources in the virtual environment a visual feedback is provided according to an application logic that at the moment is delegated to a proper script, but in future could be link to an external logic (a similar approach is still under development using Unreal Engine, maybe in future it will available in a new repository).


### Acknowledgements:

This project was inspired by a previous work focused on GIOVE framework (see [GIOVE Viewer](https://ieeexplore.ieee.org/document/7462046) and [GIOVE Virtual Factory](https://link.springer.com/chapter/10.1007/978-1-84996-172-1_12), carried on in several research projects at CNR-ITIA, now [CNR-STIIMA](http://www.stiima.cnr.it). The GIOVE Virtual Factory tool was exploited also as [an ontology-based virtual factory tool](https://www.scopus.com/record/display.uri?eid=2-s2.0-85045567162&origin=resultslist).

**This project includes [AsImpL](https://github.com/gpvigano/AsImpL) (in folder `OpenVISDIVE_Unity/Assets/AsImpL`) and a slightly modified version of [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) for loading 3D models at run-time (in folder `OpenVISDIVE_Unity/Assets/UnityGLTF`).**


### Contributing

Some feedback from you is welcome, but please notice that **this is a proof-of-concept**, thus it could be difficult at this stage to accept pull requests or to review bug notifications!

## Licensing

The project is licensed under the [MIT](https://opensource.org/licenses/MIT) license. This means you're free to modify the source and use the project in whatever way you want, as long as you attribute the original authors.


