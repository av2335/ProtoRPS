# ProtoRPS - A Prototype Rock/Paper/Scissors AI

## Contents

1 - RPS Theory
2 - The Scheme
3 - State of the Project
4 - Road Map

## RPS Theory

Rock/Paper/Scissors should not be a winnable game, because there is a simple strategy which cannot be beaten:  Just pick randomly.  You might beat a random number generator at RPS by luck, but you can't beat it by skill.  Your odds will never be better than a coin flip.

Similarly, if you were to use this strategy yourself, you couldn't be beaten either.  Pick every move randomly, and no strategy your opponent chooses will give him an advantage.

But that's not how we play, is it?  We try to predict what our opponents will do.  We try to outthink them.  Which is only possible because we know they will do the same.

How do we know that?  Even if our opponents do choose the purely random strategy, that strategy is difficult for humans to execute.  Humans are poor generators of random numbers.  See this light read on that topic:
http://scienceblogs.com/cognitivedaily/2007/02/05/is-17-the-most-random-number/

Truly random numbers only need two criteria.  The odds of each outcome must be equal, and each number must be completely independent of the previous numbers.  Since human beings are bad at random numbers, by definition, the output they generate when they try must violate at least one of these two criteria.  Furthermore, violations of either kind are statistically detectable.

The point of the project is to detect those anomalies and use them to beat humans at Rock/Paper/Scissors.

## The Scheme

The goal is to find patterns in the choices people make in a game of Rock/Paper/Scissors, and respond in a way that gives us an advantage.  This goal raises a lot of questions starting with the word "how."  How do we characterize a player's decisions?  How do we know what information is pertinent?  How do we boil the statistics down to action choices?

We'll start by realizing we don't know much yet.  We don't know what statistics will be predictive, what best win rate is possible, or to what extent our own non-random behavior will be detectable or exploitable to our opponents.  So, the plan is for the program to learn all of that for us through experience.

The program is adaptive, altering probability of each move based on the moves leading up to it.  When it selects a move, it stores metadata which includes how that move was chosen.  If that move was successful, the algorithm is rewarded - which means it slightly increases the probability that it will select that move again in the same situation in the future.  But it also increases the probability that it will use that same selection process again in the future.

For example, suppose the program elected to consider a history length of 2, meaning that it used the two previous moves in its analysis.  It saw that the opponent had thrown Rock the two previous moves, and based on that information, the program chooses to throw Paper for the current move.  If it wins, the program will be more likely to throw Paper after the opponent throws two Rocks.  Also, it will be more likely to consider move histories of length 2, and to choose moves based on the opponent's moves.  Otherwise it will be incrementally less likely to do any of those things.

### History Length

How many moves previous do players keep in mind?  How many do they consider when making their next move?  For this program, I have assumed the upper limit of that question is 3.  So, it is capable of analyzing histories of length 0 through 3.  The longer the histories go, the more complex the data collection and analysis is, so for the time being I intend to leave it this way.  If, however, the program ends up learning that histories of length 3 are better predictors than shorter histories, it may be worth revising this assumption.

### Move Type

You throw three Rocks.  On the next move, you throw a fourth Rock.  How would you characterize the choice you made?  You could say, simply, that you chose Rock.  You could also say that you chose to repeat the previous move.

More to the point, what do you expect your opponent to do, after he throws three Rocks?  It's likely you don't think about the possibilities in terms of just Rock, Paper, and Scissors.  The question is, will he continue the pattern, repeating the previous move, or will he change it?   If he does change, which way will he change it?

So, there are at least two ways you could characterize a move, as part of a sequence of moves.  Absolute moves: Rock, Paper, and Scissors.  Or, relative moves, which we will need to come up with names for: Repeat the previous move, move up the chain (from Rock to Paper), or move down the chain (from Rock to Scissors).

Furthermore, we could characterize a move played by how it related to the other player's move.  If I play Rock and you play Scissors, I played a winning move and you played a losing move.  Does your decision making process change if you are on a streak of 5 wins vs. if you are on a streak of 5 losses?

Hard to say, but we could build a computer program to find out experimentally.

### Move Choice

We could think of moves we choose similarly to how we think of moves that have already occurred.  There are the same absolute and relative moves:  Rock, Paper, Scissors, repeat the previous move, or change it.  We could also characterize choices by how they relate to the opponent's previous move.  Do we mirror that move, or play over it, or play under it?

For that last method, the program only considers absolute moves.  If the opponent plays Rock, do we play the same on the next move, or change?  The rabbit hole could go much deeper, though.  For example, if the opponent's last move was a repeat, we could choose either to match that move - by electing to repeat our own previous move - or not.

But at this point it feels like things are getting a little too convoluted, so....

### The Rest

We could catch all of the ways of analyzing moves alluded to above in one simpler scheme, by tracking all possible moves and move histories in one big pot.  Instead of considering only our own previous two absolute moves, or only the opponents' previous three relative moves, we could make use of all of it:  Our last three moves and the opponents' last three.  Or even longer, if we want.

Instead, the various different ways of analyzing and choosing moves are decoupled.  We might miss some strange patterns, like what a player does after playing Scissors, then losing the following round, and then seeing his opponent repeat a move the round after that.

But at the end of the day, this is a prototype.  It's sort of a guess at what will and won't be good predictors.  Decoupling might give some insight, moving forward, about what paths are worth going down, and what paths aren't.  Is it better to pick moves based on the win/loss record, or based on the pattern of what the opponent is shown?  Is it worth considering 3 previous moves, or are 2 enough?  If the AI is successful, then the state of it, fully trained, will be made of answers to those questions.

## The State of the Project

This project is still in its early development.  I'm getting data structures and basic logic in place, hoping soon to have a version that works at all.

For now, it's really just my thing.  I'm open to suggestions, and criticism of the coding style and general organization, but for now, it's an exercise for me to improve my own coding and design, so too much help would defeat the purpose.  I have no idea if I'll open it up for others to contribute in the future.  For now it is just an experiment.

Many things are hard coded currently.  The algorithim wired up to only be able to handle 3 moves worth of history, and already the array-based data structure is a touch confusing.  For longer histories, especially arbitrary-length ones, it might be necessary to switch to another structure.  Possibly a hash map.

## Road Map

1 - Get to a functional prototype with a rudimentary main function for smoke testing (almost done)
2 - Implement ISerializable so that instances of the AI can be saved and loaded
3 - Make a few more test drivers
     a - AI vs. Player console app
     b - AI vs. Random Number Generator
     c - AI vs. AI
4 - Make a simple website to start getting users and training data
5 - Make a more robust website, with profiles and individual instances of the AI
6 - Figure out how to meaningfully aggregate the data from multiple players
7 - Statistically analyze data to identify attempts to mettle with the data
8 - Revise or rework the algorithm as patterns emerge
9 - Expose the functionality through a web API and a mobile app

So yeah.  still a long way to go.