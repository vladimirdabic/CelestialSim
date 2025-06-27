# CelestialSim
A gravitational N-body simulator implemented in C#, modeling celestial mechanics using Newtonian gravity with numerical integration. For a more detailed readme, check out the [PDF](README.pdf) version.

## Simulation Parameters

$\Delta t$ is the time step. A smaller time step yields more precise results, but more computational power is required.

$G$ is the universal gravitational constant.

$\Delta$ represents a "fast-forward" mechanism. Because the simulation is in real time, it might take a long time to observe a significant change. In that case, $\Delta$ is added to the total elapsed time since last calculation $\epsilon$, which in turn increases the number of calculation steps $n$. When you use fast-forwarding, the simulation calculates all the skipped steps while keeping the same small time step (Δt) for accuracy.

## Controls
- Scroll: Zoom
- Left click + drag: Pan view
- Left click a body: Select
- Middle click: Add new body