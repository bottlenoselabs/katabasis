# The vision for Katabasis; Part 2: Reflections & Architecture

### Written by [@lithiumtoast](https://github.com/lithiumtoast) on Saturday, Feburary 20th, 2021

This dev log is part of a multiple part series about the vision for Katabasis.

- [Part 1: The vision for Katabasis; My past](2021-02-10_vision-for-katabasis-part-1-my-past.md)
- Part 2: The vision for Katabasis; Reflections & Architecture

## Intro

In the part 1 of of this dev log series I was mentioning how I grew into [MIT hacker culture](https://en.wikipedia.org/wiki/Hacker_culture) and it's [hacker ethics](https://en.wikipedia.org/wiki/Hacker_ethic#The_hacker_ethics). This dev log will be about how I envision these beliefs and attitudes around hard work, creativity, and joy in creating software can be used to for game-dev and beyond.

Let's start with some reflections about our modern life on planet earth in 2021. I will use these reflections as a starting point for discussion about what we can possible do about the problems in our crazy world and how game-dev fits into the picture. These reflections may not be true, but I do believe them to be of common sense.

## Reflections

1. The application of hardware and software runs the majority of our lives. 
2. The demand for human developers is increasing at an exponential rate.
3. The supply of humans developers is lower than the demand.
4. Disprutive technologies including Artificial Intelligence, Autonomous Vehicles, Fintech, DNA Sequencing, Robotics, and 3D Printing are tearing at the seams of society.
5. Since there is a perpetual amont of new developers, there is not only a lack of technical knowledge in the industry but also a lack of moral knowledge.
6. Disruptive technologies could be used to do good things, or bad things; it's upon us developers to decide.
7. If we developers do not regulate ourselves, governments will regulate us.
8. There is gathering interest among aspiring developers to create video games.
9. Game-dev can be used as a catalyst for developers to learn Computer Science, Software Engineering, and other academic disciplines traditionally only taught in settings such as a college, university, or institute of technology.
10. The open-source culture appears to be a subset of MIT hacker culture and that MIT [hacker ethics](https://en.wikipedia.org/wiki/Hacker_ethic#The_hacker_ethics) seems to resonant well amoung open-source communities.

## Vision

The idea for Katabasis is to combine these reflections and ask the questions: What if open source game-dev could be a natural bridge between aspiring developers and professional craftsmanship of developers? Could these aspirations for creating a virtal world be used as portal for creating a better physical world? Could a discipline for the art and engineering of creating video games be used for science and culture? Is real life a video game?

## Architecture

To make this vision come alive, the some knowledge, skills, and tools are necessary. For programming, I am planning to use a mix of [Zig](https://ziglang.org) and C#. The idea is for the *framework* code to be written in Zig and the *game* code to be written in C#. The *engine* code made be written in either Zig or C# depending on the context. The stack is to be open-source from top-to-bottom, no excuses.

### Zig

Why? Because it's open-source and there is growing movement amoung game-dev circles for:

1. Statically linked libraries.
2. Minimal dependencies.
3. Startup times in double-digit milliseconds.
4. Smaller sized build artifacts.
5. Zero tolerance for hidden costs during runtime such as garbage collection or just-in-time compilation.

Of course C could be an appropriate language here. The problem is that programming in C is not very fun and takes quite a bit of skill. To get more people onboard who are not familiar with low-level programming, C is most likely going to cause more friction than what would otherwise be desired. The risk of course is that Zig is a new programming language and that even less people know Zig than C. But I am confident in accepting that this risk is worth it long term. In this context, Zig is a better language replacement to C for this project.

I also have an experiment where I have a C to C# bindings generator on GitHub: https://github.com/lithiumtoast/c2cs. I plan to use the fact that Zig can generate a C header file for interopability. This would allow creating automatic bindings between Zig and C#.

### C#

Why? Because there is exists a rich history of [hacker culture](https://en.wikipedia.org/wiki/Hacker_culture) with Mono/Xamarin and MonoGame/FNA towards Microsoft's tactics of Embrace, Extend, and Extinguish (EEE) in regards to .NET and XNA, especially before Satya Nadella became CEO. (Of course still doubt whether modern Microsoft is now just in another phase of EEE.) There is also a rich past of ~20 years with .NET for business and ongoing growth today for .NET in business. If there is any hope of reaching a wide audience of developers, there has to be a on-ramp from biz-dev to game-dev. Other factors include that C# is getting attention in the past few years (thanks to the open-source community) to become more enticing for real-time systems. Examples of such advancements are spans, static function pointers, and Native AOT (formely CoreRT). Sadly there is also advancements in C# which are not very useful at all for game developers but those can be ignored and avoided.

All that being said, C# is probably not the best choice from a purely technical perspective for it's history with Java. The risk of using C# for game-dev is that there is an up-hill battle against the dogma of Object Oriented Programming (OOP). The fact that the industry was/is being influenced into thinking that Object Oriented Programming (OOP) is *the* only way to do programming is 100% a sin that can not be forgotten. However, I see this an opportunity. Developers who are familiar with OOP in C# have a new world to learn without switching languages or tools. This makes C# actually a pretty good choice for adoption as the [hacker culture](https://en.wikipedia.org/wiki/Hacker_culture) is intuitively exposed without much friction. The discussion of when and when not use OOP becomes a nuance that helps developers see there is more to learn than they originally accepted or grown acustom. I think this helps developers learn some humility as they travel the graph of the Dunning-Kruger effect from, Peak of "Mount Stupid" to Valley of Despair and then start to climb the Slope of Enlightenment. It's my opinion that once a developer has gone through the cycle of the Dunning-Kruger effect at least once they will be more aware and accepting if they happen to be on Mount Stupid next time. In this sense, C# is the perfect fit as it attracts developers who have some experience but then are likely to fall into the Valley of Despair sooner or later when they do game-dev.

### Information

Is it not sufficient to have great tools and libraries, to make this vision come alive I also need to dive deep into a broad amount of articles, books, papers, audiobooks and videos. There is a bunch of people smarter than me out there and I can listen and digest what they have to say to make me effectively a bit smarter. For that I spend a large chunk of cash in continuous learning such as:

- https://www.pluralsight.com
- https://www.masterclass.com
- https://www.thegreatcourses.com
- https://www.audible.com

I plan to write down the ideas I learned in various dev logs as a form of teaching to solidify my learning as well as to share knowledge. I'm planning to comb through [Will Wright's master class on Game Design and Theory](https://www.masterclass.com/classes/will-wright-teaches-game-design-and-theory) again soon with a dev log by this summer.

## Coincidence?

The meaning of the word *katabasis* as a atrip to the underworld can be compared to the Valley of Despair from the Dunning-Kruger effect. To be considered a a true *katabasis* it must be followed by an *anabasis* which can be compared to the the Slope of Enlightenment. Once the cycle is complete it may start again. This also shares similarties with the psycology of [fixed vs growth mindset](https://en.wikipedia.org/wiki/Mindset#Fixed_and_growth_mindset):

1. People can continue to learn and get smarter.
2. IQ is not a fixed thing and can constantly be improved by anyone.
3. Anyone who believes otherwise or that they are special are in a "fixed mindset".
4. Anyone who believes they can improve their intelligence/skills/knowledge is in a "growth mindset".

## The Future

It's not 100% clear yet how game-dev fits into the picture of making the world a better place. To me it's just this hunch, a feeling of intuition. It is clear that game-dev can be a portal to Computer Science. It's not clear how video games can be used to advance science and culture. Some others have started discussing this possibility space such as Jonathan Blow's talk on [Video Games and the Future of Education](https://www.youtube.com/watch?v=qWFScmtiC44).

