# MonoGame.Extended handoff: the roots of Katabasis 

### Written by [@lithiumtoast](https://github.com/lithiumtoast) on Tuesday, January 12th, 2021

This dev log is about the relationship between MonoGame.Extended and Katabasis. 

MonoGame.Extended is an important part of the story for Katabasis because it gives context of why MonoGame was essentially forked (it's actually FNA that was forked). MonoGame is pretty bare bones, so some people had a good idea to write and share common functionality to make MonoGame easier to develop games. I had previously [written code for MonoGame.Extended starting back in early 2016](https://github.com/craftworkgames/MonoGame.Extended/graphs/contributors). Note that MonoGame.Extended started in July 19th, 2015. I also have been [posting over on https://community.monogame.net since 2016](https://community.monogame.net/u/lithiumtoast/summary) as well. You can also see my first post on the forums:

>My name is Lucas. I’m a student at a university in Canada studying Computer Science / Software Engineering. I’m currently 22 years old, but have been programming since I was 12 years old out of self-interest and self-discipline. I originally learned how to code in Visual Basic 6 from reading and contributing to a open source project called Mirage Source and it derivatives such as Konfuse Engine / Elysium Engine. At one point, around age 15, I attempted a port of Mirage Source from Visual Basic 6 to Visual Basic.NET with some success but it kinda collapsed and over time I lost my backed up copy of the code. From then I learned C#, found XNA, started university and overall learned a lot about programming, computer science and software engineering in general.

Note that in the [previous dev log](2020-12-04_starting-dev-log.md) I mentioned that the name Katabasis is a tribute to my childhood around the time I was programming in Visual Basic 6 with Mirage / Konfuze / Elysium source code. Sharing code online for game development is pretty much inline with my childhood of sharing copy-paste tutorials on phpBB forums with Mirage / Konfuze / Elysium back before the world really started using Git and before GitHub existed.

Anyways, a lot of the problems with MonoGame.Extended were actually problems with MonoGame. Over time I become more and more frustrated with MonoGame and put less and less effort into MonoGame.Extended. Once MonoGame.Extended was handed over to me this would eventually lead to a rational that a fork of MonoGame is necessary to move forward into the future. Thus, Katabasis was born.

Here is the exchange of emails between me and [@dylanwilson80](https://github.com/dylanwilson80) over the subject of a new owner for https://github.com/Craftworkgames/MonoGame.Extended.

### On April 5th, 2020, at 9:40AM UTC-5 I started the conversation with the first email. Note that he is in the UTC-5 timezone.

> Hey Dylan,
> 
> `lithiumtoast` here.
> 
> If no one else steps up to take a over MG.Ex I be willing to. I rather someone else does though.
> 
> -L

### He responded on April 6th, 2020, at 3:42 PM UTC+10 (12:42 AM UTC-5):

> Hi Lucas,
>
> Thanks for letting me know. I actually had a feeling you might respond to the notice I put up. You've been a strong part of the MG.Ex community for quite some time. Thanks for all your help over the years.
>
> I understand your reluctance to take over. I'd much rather hand the project over to someone who is super keen to do it so let's wait and see if anyone else steps up.
>
> In an ideal world the right person would be someone who's already using the library in their game and has a significant investment in seeing the project survive. Perhaps they've even created their own fork or something.
>
> Also, I think the right person would have a vision for the future of MG.Ex. Maybe some thoughts on what should be kept and what should be thrown away, or split out into different repositories. It has crossed my mind many times that MG.Ex is trying to be too many things at once. 
>
> For example, you could easily pick any one thing and split it out into it's own library. It would be much easier for a single person to manage a library purely dedicated to Tiled or Animations or Particles. If I started over, I think this is how I would do things.
>
> Cheers
> Dylan

### I responded back on April 6th, 2020, at 10:42 AM UTC-5:

> Having a game drive the development would be important imo.
>
> As far the vision, a good place to look is how other people are doing things.
>
> * The .NET team has recently grouped a whole bunch of repositories into a 6-8 main ones. Another example is Dapper which has one repository despite having many NuGet packages. I think here having one repository is good and adding more just add confusion especially for managing GitHub issues.
>
> * NuGet packages can be hierarchical. For example, `Microsoft.AspNetCore.App` is a NuGet package which pulls in a whole bunch of other NuGet packages. However, people can still manually pick and choose the individual packages if they want by not using the App package.
> 
> * Documentation tools have developed for .NET projects over the years. See https://github.com/dotnet/docfx
> 
> * The port of ImGUI (https://github.com/mellinoe/ImGui.NET) allows support with MonoGame or Veldrid. I think refactoring the code to support switching out to other 3D app frameworks could boost the project and give more value to users. Another example of this is OpenWheels made by one the lead MonoGame contributors (https://github.com/Jjagg/OpenWheels)
>
> -L

### He responded back April 7th, 2020, 10:20 AM (UTC-5):

> Hi Lucas
>
> So far, I've had 2 other people reply to my notice seeking a new owner for MonoGame.Extended. I think the next step is to get to know each of you a little better with some questions.
>
> First, maybe tell me a little about yourself?
>
>In regards to MonoGame.Extended:
>
> - Are you currently working on a game or other project that could help drive development?
> - What do you see as the biggest problems right now?
> - If you could add something new, what would it be?
> - If you could throw something away, what would it be?
> - What do you think about the idea of splitting the library up into different repositories?
> - If the library was split up, would it make sense to have different owners for each repository? 
> - Do you have a vision for the future of MonoGame.Extended?
> - Do you have any questions for me?
>
>Also, here are the answers to some answers to questions I've received so far.
>
>#### What is involved day to day?
>
>You can spend as much or as little time as you want working on the code and documentation. Over the last 5 years I averaged between 10 and 20 hours per week working on the code. However, it's easy enough to take a long break if other things are happening in your life.
>
>Each week there's usually a handful of questions come in from the community via the MonoGame forums, Discord chat and sometimes StackOverflow. I often answered these questions but there's also quite a few members of the community that step in a answer these questions before I get to them.
>
>The other thing that comes in from time to time is pull requests on Github. Most of them are small bug fixes and can be easily eyeballed. Of course, there's also PR's for larger features that can take some time to test and discuss.
>
>
>#### Will this also include your other projects like WpfCore?
>
>I have some other open source MonoGame / game development related projects on Github. Most of these are very small in comparision to MonoGame.Extended. People do use them but the questions and PR's are much rarer for these projects.
>
>If you have a strong interest in taking over one or more of these other projects just let me know and we can discuss. 
>
>
>#### Do you have an active community working with you? 
>
>The community is definitely there although it's often hard to gauge how many people are actively involved. Some people post questions and answers regularly enough to know they are using MonoGame.Extended in their projects.
>
> As of right now there is:
> - Github - 62 watching / 766 stars / 224 forks
> - Discord Chat - 75 online / 186 offline
> - MonoGame Forums - 182 questions / roughly 100k+ views
> - MonoGameExtended.Net website - about 400 views per month
> - Patreon - 14 patrons / $29 per month
>
>Regards
>Dylan

### I responded back on April 7th. 2020, at 12:10PM (UTC-5):

> 1. Are you currently working on a game or other project that could help drive development?:
>
>I’m building a wrapper for Sokol (https://github.com/floooh/sokol) to continue my game development. So to answer your question directly, no, I’m not currently working a game, but I plan to in the near future.
>
>2. What do you see as the biggest problems right now?
>
>I think it’s the visibility and documentation. A tutorial like getting started series would problem help a lot. Something like create your own game as a series of blog posts. Something like: https://gamedevelopment.tutsplus.com/tutorials/make-a-neon-vector-shooter-in-xna-basic-gameplay--gamedev-9859, just as an example. There a bunch of open source games that have potential to be developed in a series of “how to make your own” such as https://github.com/ricardodalarme/CryBits, https://github.com/MovingBlocks/Terasology, https://github.com/OpenRCT2/OpenRCT2, https://github.com/OpenRA/OpenRA, etc.
>
>3. If you could add something new, what would it be?
>
>An alternative to the content pipeline. I know we discussed this before.
>
>Another thing would be getting more automation in for code quality. One example would be integrating https://codecov.io as a plugin for the repository on GitHub. There are a bunch of other great free tools out there for open source projects too.
>
>I also think sharing the ReSharper settings as a `.Settings` file for each solution or csproj would help people keep the code to a specific standard.
>
>4. If you could throw something away, what would it be?
>
>The key listeners. I understand that some people like do event based programming in regard to input, but I think a query based approach is more natural for game dev. Something like the following: https://gist.github.com/lithiumtoast/e3f07382f1605d56958a4700b964c484. You can see my prototype of the code here: https://gist.github.com/lithiumtoast/4457608f55123873ccde4ef99294b684. Notice how each button state can also has a timer to know how long each button is pressed down for.
>
>5. What do you think about the idea of splitting the library up into different repositories?
>
>As I told you in the last email, I think splitting into multiple repositories can be tricky for GitHub issues. I would only recommend it if the scope of the code was quite large. But then each repository would most probably benefit from being stand-alone itself. What I mean by that is people can just use that repository with MonoGame and don’t need the “core” of MonoGame.Extended. This probably makes a lot of sense for the Tiled code as the Tiled map editor keeps evolving to have more and more features.
>
>6. If the library was split up, would it make sense to have different owners for each repository? 
>
>I think in general, it would probably make sense to have at least more than one person contributing to each repository. Whether multiple people own the different repositories I’m not so sure. To make that work, a open source GitHub organization would probably have to created that owns the repositories. This would make switching of hands for the one or multiple repositories easier in the future.
>
>7. Do you have a vision for the future of MonoGame.Extended?
>
>See my previous email in the included chain.
>
>8. Do you have any questions for me?
>
>Mostly questions about the stuff you have been working on in the more private scope such as the code you write for your >Patreon members. What are the projects? How are they doing? Would you be willing to open up these projects? What is your overhead in terms of customer support?
I think the model of paying people money for priority support and requested features with the software through contact, such as direct email or discord DM, would be better than exclusive features.
>
>-L

### He responded back on April 10th, 2020, at 9:13 PM (UTC-5):

>Hi Lucas
>
>I remember you said you'd rather someone else step up. I've been discussing how to move forward with 2 others. They both look promising in terms of having games that they work on using MonoGame.Extended.
>
> - One of them has already accepted the role on paper, but there's still a lot of detail to work through.
> - The other guy hasn't replied to my last email in a few days but if he's still interested he also has a good candidate.
>
>So right now there's no reason to think you'd need to take it on. However, I'd still like to keep you in the loop. 
>
>While I've got your attention I do have one other thought on my mind.
>
>What to do think about the idea of forming an alliance so to speak? Rather than just having 1 person with complete control it could perhaps work as a collaboration. I would still prefer to step down from the head but maybe the 4 of us could get together and discuss it?
>
>Cheers
>Dylan

### I responded on April 12th, 2020, at 9:38 AM (UTC-5):

>Yes, I think having at least 2 people to a small team of no more than 5 would be a good idea.
>
>What are the timezones of everyone? I know you are in Australia, but is it Sydney? The time there as I write this already almost midnight. In the past year I moved to Montreal, the east side of Canada. It is almost 10am here as I write this (EST).
>
>-L

### I didn't here back from him for a while so I decided to send a follow up on May 1st, 2020, at 5:36 PM (UTC-5):

>Hey Dylan,
>
>Just wondering what the situation is with the project.
>
>Regards,
>Lucas

### He responded the next day on May 2nd, 2020, at 8:15 AM (UTC-5):

>Hi Lucas
>
>To be honest, it hasn't been my highest priority lately. I spoke with a few people about taking over the project and even though a couple of the candidates looked promising at first I didn't end up handing it over to anyone yet. 
>
>One guy was pretty keen to do it and I'm sure he would be technically capable but he has no obvious experience with open source and no online presence that I could find. We discussed this back and forth for a bit but ultimately nothing came of it.
>
>On a slightly positive note, Jeff Brooks (jeffgamedev) has been helping out on the Discord server but he said he wasn't interested in doing any more. Ironically, he's actually moved his game from MonoGame to Unity because of various bugs in MonoGame that he wasn't able to resolve.
>
>In some ways, I'm kind of hoping one way or another MonoGame.Extended will get taken over naturally. People are starting to get the idea that it's not being actively worked on and it's very likely that anyone interested in it will end up either forking the project or taking the bits they want and integrating them into their own project directly.
>
>Eventually, someone will come along with a better alternative and make a brand new project and people will migrate to that instead.
Of course, there's already Nez and a bunch of others: https://github.com/aloisdeniel/awesome-monogame#engines
>
>That said, the offer still stands if you want it. You've been one of the most valued contributors over the years and I'd be more than happy to leave it in your hands. The project still has a sizeable number of followers and as far as I can tell many of them would be pleased to see it become active again even in a limited capacity. It would be up to you how to move the project forward and I'm sure you could shape it to suit your lifestyle.
>
>One last thing. The monogameextended.net domain expires on 20 May 2020 so I'm going to need to figure out what to do with that. I'm happy to renew it for another year if you think having the web property is valuable, otherwise, we can just let it lapse.
>
>Regards
>Dylan

### I send back one last email on May 3rd, 2020, at 5:43 PM (UTC-5) before we moved our conversation to Discord and started writing up a Google docs for a handover:

>Hey Dylan,
>
>> One guy was pretty keen to do it and I'm sure he would be technically capable but he has no obvious experience with open source and no online presence that I could find. We discussed this back and forth for a bit but ultimately nothing came of it.
>
>Perhaps this fellow and I could team up?
>
>> On a slightly positive note, Jeff Brooks (jeffgamedev) has been helping out on the Discord server but he said he wasn't interested in doing any more. Ironically, he's actually moved his game from MonoGame to Unity because of various bugs in MonoGame that he wasn't able to resolve.
>
>Be interested in knowing more what these bugs are. Perhaps I could address them?
>
>> In some ways, I'm kind of hoping one way or another MonoGame.Extended will get taken over naturally. People are starting to get the idea that it's not being actively worked on and it's very likely that anyone interested in it will end up either forking the project or taking the bits they want and integrating them into their own project directly.
>
>IMO, the only “natural” way would be if someone created a similar project under a different name. The name “MonoGame.Extended” has a fairly well known brand (for better or for worse). One option could be to simply let it die and change the readme’s to point to the different named repo.
But I do think letting the brand die is a waste of an opportunity, regardless of the content behind the name. 
>
>That all being said, I would be willing to take ownership and team up with whoever else you have that was interested. I think it’s important to draw in a whole bunch of ideas, vision, ambitions, etc, and write them down somewhere like a google docs before any action is implemented.
>
>As for the domain name, I could CNAME it to GitHub pages.
>
>Regards,
>Lucas

