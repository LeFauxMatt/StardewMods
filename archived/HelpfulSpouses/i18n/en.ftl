## Birthday Gift

birthday-shoppinig =
    I went shopping for {$who}.
    I think {$pronoun ->
        [him] he'll
        [her] she'll
        *[other] they'll
    } {$gift-taste ->
        [like] like
        [love] love
        *[other] appreciate
    } the gift I got for {$pronoun}!

-greetings = {$time-of-day ->
    [morning] Good morning {$endearment-lower}
    [afternoon] Good afternoon {$endearment-lower}
    [evening] Good evening {$endearment-lower}
    *[other] Hi {$endearment-lower}
}

cook-a-meal =
    { -greetings }, I made some {$random-delicious} {$item-name} for {$time-of-day ->
        [morning] breakfast
        [afternoon] lunch
        [evening] dinner
    }. I hope you enjoy it!

feed-the-animals =
    { -greetings }, I fed all of the animals today.

love-the-pets =
    { -greetings }, I filled {$pet-name}'s water bowl today.

pet-the-animals =
    { -greetings }, I pet all of the animals today.

repair-the-fences =
    { -greetings }, I repaired some fences.

water-the-crops =
    { -greetings }, I watered the crops today.

water-the-slimes =
    { -greetings }, I filled the slime troughs with water.