Effort:
- Glass Shaders for bird and world box mesh created by altering alpha value of albedo color
- Bird mesh created using improved method of placing connected rectangles across control points and then loop LoopSubdivision
- implemented 4 flocking forces and tuned their weight
- Flock class used object pools to avoid fps drop from creating and destroying objects every time the flock size is changed. 
- collision avoidance force takes the world box dimensions into account as walls/ obstacles and allows for boids to smoothly avoid colliding with the wall naturally instead of bouncing off. 
- flock centering weight is very high to show flock centering at small numbers (30 boids, and 150 boids), however with 150 boids and with flock centering not enabled the velocity matching weight leads to a more realistic looking flock of birds. 