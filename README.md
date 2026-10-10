# Online Store
## 📌 Bonus Questions & Answers
Bonus (b): Explain the N+1 problem and show where it would appear if question 20 were written with a foreach loop

1.What is the N+1 problem?
It is a well-known performance anti-pattern in ORMs (like EF Core).
It occurs when the application executes 1 initial query to fetch a parent collection, followed by N separate subsequent queries to fetch related child data.
This results in a total of $N + 1$ database round trips, which heavily degrades performance.

2. Where it would appear in Question 20
:If you fetched all customers using a single query.
And then iterated through them using a foreach statement to fetch each customer's latest order individually inside the loop.

3. The Correct Approach:
Using a single joined query with proper projection (Select) or eager loading (Include) to retrieve all necessary data in a single round trip.

Bonus (c): Why can't Select((x, i) => ...) (with an index) be translated to SQL in question 22, and what is the fix?

1. Why it cannot be translated to SQL:
   . No Deterministic Order: Relational databases do not guarantee a fixed row order unless an explicit ORDER BY clause is used.
   . EF Core Limitation: EF Core cannot translate the C# sequence index parameter (i) into a standard SQL clause across all database engines. 2. The Fix:
  . Fetch and order the data into memory first by materializing the query using .ToList().
  . Then, apply the indexed Select((x, index) => ...) operation client-side in C# memory.
