# Platformer-2D 
Project made by Sebastian Kloch and Rafał Kot
<p align="center">
<i>Gameplay preview</i>
</p>
<p align="center">
<img src="images/LoH Beta 0_1_1_2 preview part 1.gif" alt="beta part 1"><br>
<img src="images/LoH Beta 0_1_1_2 preview part 2.gif" alt="beta part 1">
</p>
<br>
Link to installation file <br>
https://drive.google.com/open?id=0B7_xRkPGuWXBR3FpYTdGb0xyZlU <br>
<h2>Short development history</h2>
<h3>Idea</h3>
First idea was that game will be about a half-god who takes care of a monstrous furnace in the Earth's core.
Some day something went wrong and furnace got too much carbon and that begun main core meltdown.
And now action of game could start.
<p align="center">
<i>Idea mockup</i>
</p>
<p align="center">
<img src="images/First idea preview.gif" alt="alpha" >
</p>
<h3>Gameplay</h3>
Main target of player supposed to be running away from magma. So our hero needed some climbing skills, because he had to move up to escape. At that day me planed to make this game as endless runner.
<p align="center">
<br>
<i>First playable version of game</i>
</p>
<p align="center">
<img src="images/LoH alpha 0_0_0_1 preview.gif" alt="alpha" >
</p>
<p align="center">
<i>Expanded system of movement</i>
</p>
<p align="center">
<img src="images/LoH alpha 0_0_0_3 preview.gif" alt="alpha" >
</p>
<p align="center">
<img src="images/LoH ladders preview.gif" alt="alpha" >
</p>
After designing first levels turned out that levels with narrow vertical shafts are monotonous. Jumping from left to right and vice versa wasn't interesting. So we decided that there must be done some changes. We thought about adding special key which could open shorter paths to gain more time, but that was still without potential. So we changed story.
<p>

Now game supposed to be about young man who works in geothermal power station near Earth's core. Again something went wrong and hero must run. But this time he must slow down magma to allow evacuate people living underground, but that was still not enough.  
</p>
<p>
So there showed up idea about adding victims that during evacuation couldn't escape. Main task of player was to slow down magma, when 
victims will running away. But there wasn't many ways to add upgrades or any other solution to increase monetization of our game. So why don't add drones which will take unconscious victims. But at that day we wanted to make manual calling of drones so player would have to call drones, go slow down magma, back and call another drones and again slow down magma. This could be too repeatable and because of that we changed again assumptions.
</p>
<p>
Current story is about firefighter, who works in underground cites after The Great War, when surface of earth is poisoned by toxic gases. Action takes place in main vent of Yellowstone volcano after colling down of earth core as a result of the massive use of geothermal energy during war.
</p>
<p>
We wanted ability to use way around victims, when player doesn't want to save victims, or simple hasn't enough upgraded drones and this is too difficult for player. Player has ability too call three types of drones: ambulance, supply, firefighter.<br>
 - ambulance drone can take victims from danger area.<br>
 - supply drone can give hero pack of ammo, bandages and other stuff.<br>
 - firefighter drone can extinguish fire in selected area.<br>
But very important was to have at least simple conflangration system.
</p>
<p>
After losing motivation to first idea we wanted to make little sanbox game.
We started prototyping.
</p>

<p align="center">
<i>Conflangration based on unity colliders<br>
and test of walls with alpha
</i>
</p>
<p align="center">
<img src="images/Loh sandbox protype 1.gif" alt="sandbox" >
</p>
<p align="center">
<i>Extinction test</i>
</p>
<p align="center">
<img src="images/LoH sandbox protype 2.gif" alt="sandbox" >
</p>

<p>
During tests at bigger scale turned out that it is imposible to do conflangration with scale that we wanted.<br>
We made flames in sandbox only as information for player that in this bulding is conflangration.<br>
We moved real fire to separate scene, but there where still recurrence of building construction.
</p>

<p>
To save more time we resigned from sandbox and focused more on missions. We also resigned from endless runner form.<br>
</p>

<p align="center">
<i>We rewrited all conflangration scripts using our own collision system</i>
</p>
<p align="center">
<img src="images/LoH conflangration preview.gif" alt="sandbox" >
</p>

<p align="center">
<i>We added tiles object pooling</i>
</p>
<p align="center">
<img src="images/LoH tiles pooling.gif" alt="sandbox" >
</p>

<p align="center">
<i>We added colliders pooling</i>
</p>
<p align="center">
<img src="images/LoH colliders pooling.gif" alt="sandbox" >
</p>
<p>
Now main target of player is not only survive but also save important people. Saving other victims is optional, but there is reward for saving them.<br>
There are also missions when we have to stop incendiary. Player has to avoid traps made by arsonist and at the end of mission he must decide if he wants save incendiary or let him die in flames.
</p>

<h2>Experience</h2>

<p>
I(as Sebastian Kloch) little regret that we made this project bigger than we planed at first but I gained a lot experience not only about game development, but also about myself. At the beginning of project I was focused on vision of final version of game. I didn't enjoy development and because of that there where two big changes in games design. But after year of development I had enough skill in programming to start enjoying writing better and cleaner code.(I'm only at the beginning of learning how to write clean code).<br>
This feeling is like you clean old rusty car which can drive, but looks awful and it is dificult to repair. When you become better and better every car you clean looks more beautiful and is easier to repair.
</p>

<h3>Skills</h3>

<p>
At the beginning of development I didn't understand how and why devide classes into smaller classes. When script which controlled our hero reached a few thousands lines of code I had to devide code to smaller pieces. I still didn't know how to do this properly so I simple used polymorphism to separate one big class to parts.<br>
<br>
Main source of knowledge how I should write my scripts was every code I had contact. Code from Unity Team, code when I was searching for answers and solutions. But my level of skills really started growing when I started reading book "Clean Code" by Robert C. Martin. In this book I found many examples of code so I could finally experiement with proper knowledge.(You can see this experiements in Drones.cs)<br>
But there were problem with that I had to work on my project so my skills are still on not satisfactory level.<br>
Now near the end of project I see how bad my first code is, but I'm happy that I have idea how it should be written.
</p>
<p>
Congratulations for the fact that you managed to reach the end.<br>
Thanks for reading!
</p>
