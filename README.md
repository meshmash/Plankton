# Plankton

[![Build Status](https://travis-ci.org/Dan-Piker/Plankton.svg?branch=master)](https://travis-ci.org/Dan-Piker/Plankton)*

*_Core library only._

## Description

Plankton is a flexible and efficient library for handling n-gonal meshes. Plankton is written in C# and implements the [halfedge data structure][hds].  The structure of the library is loosely based on [Rhinocommon][rc]'s mesh classes and was originally created for use within C#/VB scripting components in [Grasshopper][gh].

_Plankton is still in the **very early stages** of development and as such things **may** break from time to time without warning.  If you have any ideas for functionality that you would like to see in the project, please [open a ticket][issues] or get in touch via the [Plankton Grasshopper Group][ghgroup]._

## Features

* **Flexible** – Plankton can represent n-gonal meshes. There is no restriction to the number of sides that a face can have.
* **Fast** – Efficient adjacency queries (traversals) are provided by the underlying [halfedge data structure][hds].  These operations, such as finding the faces adjacent to a particular vertex, are _O(n)_ where _n_ is the number of elements returned.
* **Robust** – Several [Euler operators][euler] have been implemented (such as edge-collapsing, and face-splitting) that allow the user to modify the topology of a mesh without worrying about the specifics of the data structure.
* **Compatible** – Plankton can interface with the face-vertex mesh representation, making it straightforward to use alongside [Turtle]. 

## Future

Plankton was created as a framework for our own work with meshes in Grasshopper. Over time we hope to make it much more robust and improve its functionality with additions such as:

* dynamic properties – from mesh normals to custom attributes (such as forces on mesh vertices)
* more topological and geometrical operators – such as Conway's operators and remeshing
* subdivision surfaces
* polyline support in Grasshopper – to improve compatibility with other Grasshopper add-ons
* documentation and examples
* _proper_ support for Dynamo/DesignScript

There is, of course, a line to be drawn between algorithms that belong _in_ the library and algorithms that are simply written _for_ the library.  We hope to iron out such details in due course, along with guidelines for [contributing](CONTRIBUTING.md) to the project.

## License

Plankton an _open source_ library and is licensed under the [Gnu Lesser General Public License][lgpl] (LGPL). We chose this license because we believe that it will encourage those who _improve_ the library to _share_ their work whilst not requiring the same of those who simply _use_ the library in their software.

    Plankton is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.
    
    Plankton is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.
    
    You should have received a copy of the GNU Lesser General Public
    License along with Plankton.  If not, see
    <http://www.gnu.org/licenses/>.

## Using Plankton with Grasshopper

### Pre-compiled binaries

The easiest way to use Plankton is to download the latest [release][releases].  Both the core library, `plankton.dll`, and the Grasshopper assembly, `plankton.gha`, should be copied into Grasshopper's "libraries" folder (usually `%appdata%\Roaming\Grasshopper\Libraries\`).  Rhino will need to be restarted if it is already running and don't forget to **unblock** the assemblies!

To use Plankton from a C#/VB scripting component in Grasshopper you'll need to reference the library in the script. Right-click on the component, choose "Manage Assemblies" and using the dialog box, select _both_ of the assemblies.  See [here][scripting] for more information about writing scripts that use Plankton.

### Building from source

If you want to keep up with the latest developments, or if you wish to contribute to the project, then you'll need to compile Plankton on your own computer.  Plankton is built against .NET 4.0 so you'll need Visual Studio 2010 or later (or SharpDevelop).  Resolving the dependencies should be as easy as dropping `Grasshopper.dll`, `GH_IO.dll` and `Rhinocommon.dll` into the `lib/` folder. The solution also includes a test project which uses [NUnit].

Once you've built the library continue with the instructions for [pre-compiled binaries](#pre-compiled-binaries).

## Thanks

Thanks to Dave Stasiuk, Giulio Piacentino, Kristoffer Josefsson, Harri Lewis, John Harding, Daniel Hambleton.

***

Plankton © 2013 Daniel Piker and Will Pearson.


[ghgroup]: http://www.grasshopper3d.com/group/plankton
[issues]: http://github.com/Dan-Piker/Plankton/issues
[rc]: http://github.com/mcneel/rhinocommon
[gh]: http://grasshopper3d.com
[hds]: http://github.com/Dan-Piker/Plankton/wiki/Halfedge-Data-Structure
[license]: http://github.com/Dan-Piker/Plankton/tree/master/LICENSE.txt
[lgpl]: http://www.gnu.org/licenses/lgpl.html
[euler]: http://github.com/Dan-Piker/Plankton/wiki/Euler-Operators
[releases]: http://github.com/Dan-Piker/Plankton/releases
[scripting]: http://github.com/Dan-Piker/Plankton/wiki/Scripting
[Turtle]: http://github.com/piac/TurtleMesh
[nunit]: http://www.nunit.org
