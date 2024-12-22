# CelestialSim
This is a super basic celestial mechanics simulator implemented in C#. Movement of bodies is calculated using Newton's universal gravitational law.

### Simulation Parameters

**n** is the step count, that is, the number of steps/calculations per frame.\
**Δt** is the time step used to calculate the positions of bodies. A smaller Δt yields more precise results but more steps are required for a fast simulation.\
**G** is the universal gravitational constant. For the most part you can let G = 1.

### Controls

Click and drag to move around, select a body by clicking it.\
Middle click is used to insert a body with the specified properties.\
Mouse wheel to zoom in/out.