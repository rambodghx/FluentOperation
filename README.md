# Fluent Operation
Fluent Operation has a clear idea, **Convenient Service Call**.
When I was working on non-complex scenarios in my projects, I realized that there was repetitive code in the layer that we called **Presentation**. This repetitive part was nothing but nested `try-catch` and `if-else` on responses provided from deeper layers.
After that, I figured out my main problem in these kinds of projects is keeping things readable and clean and not using heavy libraries ;) So, I made my own standard in service call, but that is not a huge thing.

# What About Convenient Service Call?
Most of us are familiar with the **Clean** approach or at least the **Port-Adapter** architecture style.
In these kinds of styles, we consider that the layers containing our Use-Cases should be called from the top layer named **Presentations**.
As a guideline, the Use-Case layer (Application) shouldn't depend on the top layer, but sometimes this dependency is shadowed.
Shadowed?! Yes, I mean that it's not exactly keeping a reference to the top layer but caring about the **result format** for the top layer.
I don't like it because I think this is some kind of dependency.

# Usage 
The usage is going to be updated...
