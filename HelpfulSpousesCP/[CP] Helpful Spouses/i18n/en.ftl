## Dialogue

dialogue-BirthdayGift =
    It's { $Birthday }'s birthday today! I got [ArticleFor [{ $ItemName }]] { $ItemName } for you to give to { $BirthdayGender ->
    [Male]  her
    [Female] him
        }. { $ItemId }

dialogue-FeedTheAnimals =
    { $Spouse ->
    [Shane] Hey { $NickName }, I fed all the animals already. Heh, you should've seen how { $AnimalName } went to town on breakfast.
    [Emily] Good morning { $NickName }! I've fed all the animals.
    *[other] Good morning { $NickName }, I fed all of the animals today.
        }

dialogue-LoveThePets =
    { $Spouse ->
    [Shane] If { $PetName } tries to con you, I already filled their bowl. No matter how much they whine or how cute and sad they look you can't fall for it.
    [Elliott] I filled the pets' water bowl already, my love.
    [Emily] I was filling { $PetName }'s water bowl today, and they were being so sweet! They really are like a member of the family, don't you think?
    *[other] Good morning { $NickName }, I filled { $PetName }'s water bowl.
        }

dialogue-MakeBreakfast =
    { $Spouse ->
    [Shane] Hey { $NickName }, I fed all the animals already. Heh, you should've seen how { $AnimalName } went to town on breakfast.
    [Emily] Good morning { $NickName }! I've fed all the animals.
    [Krobus] Umm... I found this food lying around. I thought you might like it...{ $ItemId }
    *[other] Good morning { $NickName }, I made you [ArticleFor [{ $ItemName }]] { $ItemName } for breakfast this morning. I hope you enjoy it!{ $ItemId }
        }

dialogue-PetTheAnimals =
    { $Spouse ->
    [Shane] Morning { $NickName }. I got up early and I've visited all the chickens, and the other animals too. Gave 'em all plenty of attention. Can't believe that's even a chore we have to do, it's one of the best parts of my day! Besides you, of course.
    [Elliott] Oh, and all our animals seem happy this morning. Don't worry about that.
    [Emily] I pet all the animals this morning- they have such pure souls. I think a lot of humans forget how much they can learn from animals, don't you? I can see wisdom in their eyes that I may never understand.
    *[other] Good morning { $NickName }, I pet all of the animals in the barn and coop.
        }

dialogue-RepairTheFences =
    { $Spouse ->
    [Shane] Hey { $PlayerName }, I walked around and fixed some fences that were falling down. What? Yeah, it might not be my specialty, but I know how. Who do you think had to fix all the broken light bulbs and shelves at Joja?
    [Emily] Oh, { $PlayerName }! Some of the fences were falling down so I replaced them. They won't need repair for a little while.
    *[other] Good morning { $NickName }, I repaired some of the fences.
        }

dialogue-WaterTheCrops =
    { $Spouse ->
    [Shane] Hey, I actually had a burst of productivity today, and I watered all the crops. Fresh air was nice.
    [Emily] The crops looks so happy this morning when I went to water them. They love the soil here, I can tell.
    *[other] Good morning { $NickName }, I watered the crops for you today.
        }

dialogue-WaterTheSlimes =
    { $Spouse ->
    [Shane] Hang on, I gotta chance- I just watered the slimes and they got me all goopy. Eh, they're not that scary, I just try to envision them as chickens- gross, multiplying, angry chickens that don't want to be pet, and try to fight me the instant they see me.
    [Emily] I just filled the water for the slime hutch! Oh no, { $PlayerName }, even when they attack us it doesn't upset me. They're just doing what is instinct for them. It's part of their nature, and there's nothing wrong with that, even if we don't understand it.
    [Krobus] I like visiting the slimes, they remind me of the sewers.
    *[other] Good morning { $NickName }, I filled the slimes troughs with water.
        }