using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // max valid blockjuck score for a hand
        const int MaxHandValue = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TopCardOffset = 100;
        const int HorizontalCardOffset = 150;
        const int VerticalCardSpacing = 125;

        // messages
        SpriteFont messageFont;
        const string ScoreMessagePrefix = "Score: ";
        const string WinnerMessagePrefix = " won!";
        Message playerScoreMessage;
        Message dealerScoreMessage;
        Message drawMessage;
        Message winnerMessage;
		List<Message> messages = new List<Message>();

        // message placement
        const int ScoreMessageTopOffset = 25;
        const int HorizontalMessageOffset = HorizontalCardOffset;
        Vector2 winnerMessageLocation = new Vector2(WindowWidth / 2,
            WindowHeight / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        Texture2D hitButtonSprite;
        Texture2D standButtonSprite;
        MenuButton quitButton;
        MenuButton hitButton;
        MenuButton standButton;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TopMenuButtonOffset = TopCardOffset;
        const int QuitMenuButtonOffset = WindowHeight - TopCardOffset;
        const int HorizontalMenuButtonOffset = WindowWidth / 2;
        const int VerticalMenuButtonSpacing = 125;
        Vector2 hitButtonPlacement = new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset);
        Vector2 standButtonPlacement = new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset + VerticalMenuButtonSpacing );
        Vector2 quitButtonPlacement = new Vector2(HorizontalMenuButtonOffset, QuitMenuButtonOffset);

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public MenuButton StandButton
        {
            get
            {
                return standButton;
            }

            set
            {
                standButton = value;
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create and shuffle deck
            Deck deck = new Deck(Content, 0, 0);
            deck.Shuffle();

            // first player card
            playerHand.Add(deck.TakeTopCard());
            playerHand[0].FlipOver();
            playerHand[0].X = HorizontalCardOffset;
            playerHand[0].Y = TopCardOffset;

            // first dealer card
            dealerHand.Add(deck.TakeTopCard());
            dealerHand[0].X = WindowWidth - HorizontalCardOffset;
            dealerHand[0].Y = TopCardOffset;

            // second player card
            playerHand.Add(deck.TakeTopCard());
            playerHand[1].FlipOver();
            playerHand[1].X = HorizontalCardOffset;
            playerHand[1].Y = TopCardOffset + VerticalCardSpacing;

            // second dealer card
            dealerHand.Add(deck.TakeTopCard());
            dealerHand[1].FlipOver();
            dealerHand[1].X = WindowWidth - HorizontalCardOffset;
            dealerHand[1].Y = TopCardOffset + VerticalCardSpacing;


            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>(@"fonts\Arial24");
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(),
                messageFont,
                new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(playerScoreMessage);
            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(),
                messageFont,
                new Vector2(WindowWidth - HorizontalCardOffset, ScoreMessageTopOffset));
            
            //create finals game messages
            winnerMessage = new Message(" won!", messageFont, winnerMessageLocation);
            drawMessage = new Message("Draw!", messageFont, winnerMessageLocation);

            // load quit button sprite for later use dn create exit button
            quitButtonSprite = Content.Load<Texture2D>(@"graphics\quitbutton");
            quitButton = new MenuButton(quitButtonSprite, quitButtonPlacement, GameState.Exiting);

            // create hit button and add to list
            hitButtonSprite = Content.Load<Texture2D>(@"graphics\hitbutton");
            hitButton = new MenuButton(hitButtonSprite, hitButtonPlacement, GameState.PlayerHitting);
            menuButtons.Add(hitButton);

            // create stand button and add to list
            standButtonSprite = Content.Load<Texture2D>(@"graphics\standbutton");
            standButton = new MenuButton(standButtonSprite, standButtonPlacement, GameState.WaitingForDealer);
            menuButtons.Add(standButton);

            
            
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            MouseState currentMouseState = Mouse.GetState();


            // update menu buttons as appropriate
            if (currentState == GameState.WaitingForPlayer || currentState == GameState.DealerHitting)
            {
                foreach (MenuButton button in menuButtons)
                {
                    button.Update(currentMouseState);
                    //if (standButton == .Pressed && buttonReleased)
                }
            }

            // game state-specific processing
            switch (currentState)
            {
                case GameState.CheckingHandOver:
                    //both players stand
                    if (!dealerHit && !playerHit)
                    {
                        menuButtons.Add(quitButton);
                        messages.Add(dealerScoreMessage);
                        dealerHand[0].FlipOver();
                        //Player score or Dealer score greater than 21 
                        if (GetBlockjuckScore(playerHand) > MaxHandValue ||
                            GetBlockjuckScore(dealerHand) > MaxHandValue)
                        {
                            //Player score out of limit and Dealer score in limit = dealer wins
                            if (GetBlockjuckScore(playerHand) > MaxHandValue &
                                GetBlockjuckScore(dealerHand) <= MaxHandValue)
                            {
                                winnerMessage.Text = "Dealer" + WinnerMessagePrefix;
                                messages.Add(winnerMessage);
                            }
                            //Dealer score out of limit and player score in limit = player wins
                            if (GetBlockjuckScore(dealerHand) > MaxHandValue &
                                GetBlockjuckScore(playerHand) <= MaxHandValue)
                            {
                                winnerMessage.Text = "Player" + WinnerMessagePrefix;
                                messages.Add(winnerMessage);
                            }
                            //Both scores out of limit = draw
                            if (GetBlockjuckScore(playerHand) > MaxHandValue &
                                GetBlockjuckScore(dealerHand) > MaxHandValue)
                            {
                                messages.Add(drawMessage);
                                menuButtons.Add(quitButton);
                            }
                        }
                        //Player scores lower than 21
                        else
                        {   
                            //Player got greater scores than Dealer
                            if (GetBlockjuckScore(playerHand) > GetBlockjuckScore(dealerHand))
                            {
                                winnerMessage.Text = "Player" + WinnerMessagePrefix;
                                messages.Add(winnerMessage);
                            }
                            //Dealer got greater scores than Dealer
                            if (GetBlockjuckScore(dealerHand) > GetBlockjuckScore(playerHand))
                            {
                                winnerMessage.Text = "Dealer" + WinnerMessagePrefix;
                                messages.Add(winnerMessage);
                            }
                            if (GetBlockjuckScore(playerHand) == GetBlockjuckScore(dealerHand))
                            {
                                messages.Add(drawMessage);
                                menuButtons.Add(quitButton);
                            }
                        }
                        currentState = GameState.WaitingForPlayer;
                    }
                    else
                    {
                        dealerHit = false;
                        playerHit = false;
                        currentState = GameState.WaitingForPlayer;
                    }
                    break;
                case GameState.DealerHitting:
                    Deck deck1 = new Deck(Content, 0, 0);
                    dealerHand.Add(deck1.TakeTopCard());
                    dealerHand[dealerHand.Count - 1].FlipOver();
                    dealerHand[dealerHand.Count - 1].X = (WindowWidth - HorizontalCardOffset);
                    dealerHand[dealerHand.Count - 1].Y = TopCardOffset + VerticalCardSpacing * (dealerHand.Count - 1);
                    dealerScoreMessage.Text = ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString();
                    dealerHit = true;
                    currentState = GameState.CheckingHandOver;
                    break;
                case GameState.DisplayingHandResults:
                    messages.Add(dealerScoreMessage);
                    break;
                case GameState.Exiting:
                    Exit();
                    break;
                case GameState.PlayerHitting:
                    Deck deck = new Deck(Content, 0, 0);
                    playerHand.Add(deck.TakeTopCard());
                    playerHand[playerHand.Count - 1].FlipOver();
                    playerHand[playerHand.Count - 1].X = HorizontalCardOffset;
                    playerHand[playerHand.Count - 1].Y = TopCardOffset + VerticalCardSpacing * (playerHand.Count - 1);
                    playerScoreMessage.Text = ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString();
                    messages.Add(playerScoreMessage);
                    playerHit = true;
                    currentState = GameState.WaitingForDealer;
                    break;
                case GameState.WaitingForDealer:
                    if (GetBlockjuckScore(dealerHand) <= 16)
                        currentState = GameState.DealerHitting;
                    else
                        currentState = GameState.CheckingHandOver;
                    break;

            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);
						
            spriteBatch.Begin();

            // draw hands
            foreach (Card card in playerHand)
            {
                card.Draw(spriteBatch);
            }
            foreach (Card card in dealerHand)
            {
                card.Draw(spriteBatch);
            }
            
            spriteBatch.End();

            // draw messages
            foreach (Message message in messages)
            {
                message.Draw(spriteBatch);
            }
            // draw menu buttons
            foreach (MenuButton button in menuButtons)
            {
                button.Draw(spriteBatch);
            }
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blockjuck score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blockjuck score for the hand</returns>
        private int GetBlockjuckScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlockjuckCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MaxHandValue)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blockjuck value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blockjuck value for the card</returns>
        private int GetBlockjuckCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
    }
}
