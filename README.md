# CyberpunkPrototype

You can try it here, online: http://matteo-bevan.com/cyberpunkdemo

The "game" is an infinite runner in 3D, inside the cyberspace. The view is in first person and you are running on a cyber wire inside the cyber space. The cyberspace is randomly generated.

At the start I create 3 heightmaps. Every heightmap is random with weight to have a consistent landscape. Using the heightmaps I create Bezier curves calculating the control points and then I use the curves to generate the cylinders. I apply my glowing material with a custom shader.

When the player exits an heightmap-space I delete it and create a new one further where he cannot see yet. When he reaches the next heightmap I create a new one and so on.

I could use a pooling system (I already used them a lot of times) but for the sake of the prototype was an overkill.

The evolution of this project would be to set powerups and obstacles to avoid.

The original idea was to use this project for the movement in the cyberspace and then make a more strategic game for the combact (assault on servers and such).

Note 1: This is an old project of mine, I worked with more recent versions of UI and I made more complex Unity projects. I think this one is perfect to show my abilities though.

Note 2: in this git is present an old version of NGUI I paid. You can use or reuse my project but you cannot use the NGUI part without paying them. It's an old version though and the new Unity UI works better than that version. https://www.assetstore.unity3d.com/en/#!/content/2413
