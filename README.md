# SF Utilities
A early wip Unity utilities package that will contain general utilities for several things.
If you are looking for UI Toolkit utitlities look at the SF UI Elements. That is a dedicated UI package for Unity's UI Toolkit.

[SF UI Elements Package](https://github.com/Shatter-Fantasy/SF-UI-Elements)

<details>
  <summary>Multithreaded Procedural Mesh Generation</summary>
  
## Mesh Generation Overvieww
Credit to CatLikeCoding for teaching people how to do this.

Created a series of Mesh Jobs that implement an IMeshGenerator to procedurally generate different shapes.

IMeshGenerator defines the base implementation for executing a C# job that generates shapes procedurally.
It includes fields for Vertex and Index count for the generated mesh.

IMeshStream is used to initialize the C# Job for procedural data creation used in meshes like defining the Triangles, SetVertex positions and calculating the bounds.

MeshJob is used to schedule jobs defined in the IMeshGenerator being implemented by the chosen ProceduralMeshs and to also pass in the IMeshStream.

## Usecase
This is being used in the ocean generator I am working on.
Also useful for a work in progress sprite mesh generation tool I am working on for a custom 2D navmesh and raymarhcing system.

![Ocean Rendering 2](https://github.com/user-attachments/assets/2ee82c99-f9fe-4223-8c20-18411ac34b9b)

</details>




## Lot of Misc functionality that just helped in several cases.
1. Methods to cast between collection types that is currently not possible or had issues with using just the LINQ ToList() method.
2. Graphic Libraries utilities that include batched drawing for rendering different shapes using GL commands. Example DrawGridMarquee for drawing a set of tiles from a grid.
3. Extenstions for different structs like Rect, Bounds, and Vectors.
4. Asset Database methods to help manipulate data in assets and also easily find assets without knowing the folder they are in. 

## WIP Documentation
The documentation for most SF packages are currently early work in progress.
At the moment the API documentation has all the fields, properties, and classes appearing.
Due note they don't have full descriptions yet, but they do show the class, method names, namespaces, value type for parameters, and variables.

[SF Utilities Documentation](https://shatter-fantasy.github.io/SF-Utilities/api/SF.Utilities.GLUtilities.html)
