using System;
using System.Drawing;
using System.Windows.Forms;

// mod: setRoom1 doesn't repeat over and over again

namespace ACFramework
{ 

	class cCritterDoor : cCritterWall 
	{

	    public cCritterDoor(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame ) 
		    : base( enda, endb, thickness, height, pownergame ) 
	    { 
	    }
		
		public override bool collide( cCritter pcritter ) 
		{ 
			bool collided = base.collide( pcritter ); 
			if ( collided && pcritter.IsKindOf( "cCritter3DPlayer" ) ) 
			{ 
				(( cGame3D ) Game ).setdoorcollision( ); 
				return true; 
			} 
			return false; 
		}
 
        public override bool IsKindOf( string str )
        {
            return str == "cCritterDoor" || base.IsKindOf( str );
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterDoor";
            }
        }
	}

    class cCritterWallMover : cCritterWall
    {
        private bool moveFlag;
        private cVector3 moveAxisAndDirection;

        private static cVector3 defaultMoveAxis = new cVector3(0f, -.05f, 0f); // Default Is Verdical Down

        public cCritterWallMover(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame)
            : base(enda, endb, thickness, height, pownergame)
        {
            this.moveFlag = true;
            this.moveAxisAndDirection = defaultMoveAxis;
        }

        public cCritterWallMover(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame, cVector3 moveAxisAndDirection)
            : base(enda, endb, thickness, height, pownergame)
        {
            this.moveFlag = true;
            this.moveAxisAndDirection = moveAxisAndDirection;
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if (moveFlag)
            {
                this.moveTo(this.Position.add(this.moveAxisAndDirection), false);
            }
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterWallMover" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterWallMover";
            }
        }

    }


    //==============Critters for the cGame3D: Player, Ball, Treasure ================ 

    class cCritter3DPlayer : cCritterArmedPlayer 
	{
        //private bool warningGiven = false; //not needed at the moment
        cGame game;
        
        public cCritter3DPlayer( cGame pownergame ) 
            : base( pownergame )
        {
            game = pownergame;
            //Sprite = new cSpriteSphere(); 
            //Sprite.FillColor = Color.DarkGreen; 
            Sprite = new cSpriteQuake(ModelsMD2.mog);
            Sprite.SpriteAttitude = cMatrix3.scale( 2, 0.8f, 0.4f ); 
			setRadius( cGame3D.PLAYERRADIUS ); //Default cCritter.PLAYERRADIUS is 0.4.  
			
			moveTo( _movebox.LoCorner.add( new cVector3( 0.0f, 0.0f, 2.0f ))); 
			WrapFlag = cCritter.CLAMP; //Use CLAMP so you stop dead at edges.
			Armed = false; //bullets turned off
			MaxSpeed =  cGame3D.MAXPLAYERSPEED; 
			AbsorberFlag = true; //Keeps player from being buffeted about.
			ListenerAcceleration = 160.0f; //So Hopper can overcome gravity.  Only affects hop.
		
            // YHopper hop strength 12.0
			Listener = new cListenerScooterYHopper( 0.2f, 12.0f );

            // the two arguments are walkspeed and hop strength -- JC
            
            addForce( new cForceGravity( 50.0f )); /* Uses  gravity. Default strength is 25.0.	Gravity	will affect player using cListenerHopper. */ 
			AttitudeToMotionLock = false; //It looks nicer is you don't turn the player with motion.
			Attitude = new cMatrix3( new cVector3(0.0f, 0.0f, -1.0f), new cVector3( -1.0f, 0.0f, 0.0f ), new cVector3( 0.0f, 1.0f, 0.0f ), Position);
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt); //Always call this first 
        } 

        public override bool collide( cCritter pcritter ) 
		{ 
			bool playerhigherthancritter = Position.Y - Radius > pcritter.Position.Y; 
		
            _baseAccessControl = 1;
			bool collided = base.collide( pcritter );
            _baseAccessControl = 0;
            if (!collided) 
				return false;
            /* If you're here, you collided.  We'll treat all the guys the same -- the collision
            with a Treasure is different, but we let the Treasure contol that collision. */
            
            bool childsMode = Framework.Keydev[vk.C]; //checks to see if c is pressed

            if (childsMode)
            {
                //nothing happens, just makes sure if you collide nothing happens, CHILDS MODE
            }
            else if (pcritter.IsKindOf("cEnemyPotator"))
            {
                game.GameOver = true;
            }
            else
            {
                _loseRoom = true; //sets to true so that if collideing it goes to the lose room
            }
			 
			pcritter.die(); 
			return true; 
		}

        public override cCritterBullet shoot()
        {
            Framework.snd.play(Sound.LaserFire);
            return base.shoot();
        }

        public override bool IsKindOf( string str )
        {
            return str == "cCritter3DPlayer" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayer";
            }
        }
	} 
	
   
	class cCritter3DPlayerBullet : cCritterBullet 
	{

        public cCritter3DPlayerBullet() { }

        public override cCritterBullet Create()
            // has to be a Create function for every type of bullet -- JC
        {
            return new cCritter3DPlayerBullet();
        }
		
		public override void initialize( cCritterArmed pshooter ) 
		{ 
			base.initialize( pshooter );
            Sprite.FillColor = Color.Crimson;
            // can use setSprite here too
            setRadius(0.1f);
		} 

        public override bool IsKindOf( string str )
        {
            return str == "cCritter3DPlayerBullet" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayerBullet";
            }
        }
	} 
	
	class cCritter3Dcharacter : cCritter  
	{ 
		
        public cCritter3Dcharacter( cGame pownergame ) 
            : base( pownergame ) 
		{ 
			addForce( new cForceGravity( 25.0f, new cVector3( 0.0f, -1, 0.00f ))); 
			//addForce( new cForceDrag( 20.0f ) );  // default friction strength 0.5 
			Density = 2.0f; 
			MaxSpeed = 30.0f;
            if (pownergame != null) //Just to be safe.
               //Sprite = new cSpriteQuake(Framework.models.selectRandomCritter());

            Sprite = new cSpriteQuake(ModelsMD2.chicken);
            Sprite = new cSpriteQuake(ModelsMD2.hand);
            Sprite = new cSpriteQuake(ModelsMD2.jack);
            Sprite = new cSpriteQuake(ModelsMD2.mog); //player character
            Sprite = new cSpriteQuake(ModelsMD2.mrfrost);
            Sprite = new cSpriteQuake(ModelsMD2.oluvegold);
            Sprite = new cSpriteQuake(ModelsMD2.oluvegray);
            Sprite = new cSpriteQuake(ModelsMD2.oluvegreen);
            Sprite = new cSpriteQuake(ModelsMD2.oluvered);
            Sprite = new cSpriteQuake(ModelsMD2.oluvesilver);
            Sprite = new cSpriteQuake(ModelsMD2.oluvewhite);
            Sprite = new cSpriteQuake(ModelsMD2.penguin);
            Sprite = new cSpriteQuake(ModelsMD2.potator);

            if ( Sprite.IsKindOf( "cSpriteQuake" )) //Don't let the figurines tumble.  
			{ 
				AttitudeToMotionLock = false;   
				Attitude = new cMatrix3( new cVector3( 0.0f, 0.0f, 1.0f ), new cVector3( 1.0f, 0.0f, 0.0f ), new cVector3( 0.0f, 1.0f, 0.0f ), Position); 
				/* Orient them so they are facing towards positive Z with heads towards Y. */ 
			} 
			Bounciness = 0.0f; //Not 1.0 means it loses a bit of energy with each bounce.
			setRadius( 1.0f );
            MinTwitchThresholdSpeed = 4.0f; //Means sprite doesn't switch direction unless it's moving fast 
			randomizePosition( new cRealBox3( new cVector3( _movebox.Lox, _movebox.Loy, _movebox.Loz + 4.0f), new cVector3( _movebox.Hix, _movebox.Loy, _movebox.Midz - 1.0f))); 
				/* I put them ahead of the player  */ 
			randomizeVelocity( 0.0f, 30.0f, false ); 

                        
			if ( pownergame != null ) //Then we know we added this to a game so player() is valid
				addForce( new cForceObjectSeek( Player, 0.5f ));

            int begf = Framework.randomOb.random(0, 171);
            int endf = Framework.randomOb.random(0, 171);

            if (begf > endf)
            {
                int temp = begf;
                begf = endf;
                endf = temp;
            }

			Sprite.setstate( State.Other, begf, endf, StateType.Repeat ); ////////////////////////
            _wrapflag = cCritter.BOUNCE;

		} 

		
		public override void update( ACView pactiveview, float dt ) 
		{ 
			base.update( pactiveview, dt ); //Always call this first
            rotateAttitude(Tangent.rotationAngle(AttitudeTangent));
			
            //**********************************************************************************
            //Commented out so that collision with the HIZ does not make deletion
            //if ( (_outcode & cRealBox3.BOX_HIZ) != 0 ) /* use bitwise AND to check if a flag is set. */ 
			//	delete_me(); //tell the game to remove yourself if you fall up to the hiz.
        }

        // do a delete_me if you hit the left end 

        public override void die() 
		{ 
			//Player.addScore( Value ); 
			base.die(); 
		} 

       public override bool IsKindOf( string str )
        {
            return str == "cCritter3Dcharacter" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3Dcharacter";
            }
        }
	}

    class cHandgunBullet : cCritterBullet
    {
        public cHandgunBullet()
        { }
        public override cCritterBullet Create()
        // has to be a Create function for every type of bullet -- JC
        {

            return new cHandgunBullet();
        }
        public override void initialize(cCritterArmed pshooter)
        {
            base.initialize(pshooter);
            Sprite = new cSpriteQuake(ModelsMD2.chicken);
            setRadius(.5f);
        }
        public override bool collide(cCritter pcritter)
        {
            bool success = base.collide(pcritter);
            if (success && pcritter.IsKindOf("cEnemyHand"))
            {
                return false;
            }
            
            return success;
        }
        public override bool IsKindOf(string str)
        {
            return str == "cHandgunBullet" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cHandgunBullet";
            }
        }
    }

    
    class cEnemyHand : cCritterArmed
    {
        public cEnemyHand(cGame pownergame) : base(pownergame)
        {
            
            Sprite = new cSpriteQuake(ModelsMD2.hand);
            setRadius(1.0f);            
            Sprite.setstate(State.Other, 46, 53, StateType.Repeat); //Gun
            
            randomizeVelocity(0, 0, false);

            addForce(new cForceObjectSeek(Player, 5f));
            BulletClass = new cHandgunBullet(); //the hand will shoot chickens
            Armed = true; //so that the hand is armed with the "gun"
            _bshooting = true; //when true, the hand will constantly shoot
            //_aimtoattitudelock = true; //supposed to aim somewhat towards a target
            WaitShoot = 1;//how long it waits between every shot
           

            int positionCount = 0;
            cVector3[] positions = new cVector3[1]; //to store the positions of the hand guns
            Attitude = new cMatrix3(new cVector3(0.0f, 0.0f, 1.0f), new cVector3(1.0f, 0.0f, 0.0f), new cVector3(0.0f, 1.0f, 0.0f), Position);
                      
            positions[0] = new cVector3(-8, -5, 0);
                

            moveTo(positions[positionCount]);

            //positionCount++;

            addForce(new cForceDrag(20.0f));   
        }
        public override void die()
        {           
           
        }
        public override bool IsKindOf(string str)
        {
            return str == "cEnemyHand" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cEnemyHand";
            }
        }
    }


    class cEnemyJack : cCritter3Dcharacter
    {
        public cEnemyJack(cGame pownergame, cVector3 vmoveTo) : base(pownergame)
        {
           
            Sprite = new cSpriteQuake(ModelsMD2.jack);
            Sprite.setstate(State.Other, 46, 53, StateType.Repeat); //Change===================================
            moveTo(_movebox.LoCorner.add(new cVector3(0.0f, 0.0f, 2.0f)));
            randomizeVelocity(0, 0, false);
        }
        public override bool IsKindOf(string str)
        {
            return str == "cEnemyJack" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cEnemyJack";
            }
        }
    }

    class cEnemyMrFrost : cCritter3Dcharacter 
    {
        public cEnemyMrFrost(cGame pownergame) : base(pownergame)
        {
          
            Sprite = new cSpriteQuake(ModelsMD2.mrfrost);
            Sprite.setstate(State.Other, 0, 11, StateType.Repeat); //idle
                                                                   //Sprite.setstate(State.Other, 84, 94, StateType.Repeat); //pipe
            randomizePosition(new cRealBox3(new cVector3(_movebox.Lox, _movebox.Loy, _movebox.Loz + 4.0f), new cVector3(_movebox.Hix, _movebox.Loy, _movebox.Hiz - 4.0f)));
            addForce(new cForceDrag(10.0f)); //so that the snowmen do not move

        }
        public override bool IsKindOf(string str)
        {
            return str == "cEnemyMrFrost" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cEnemyMrFrost";
            }
        }
    }

    /*More states*/
    class cEnemyOluve : cCritter3Dcharacter
    {
        public cEnemyOluve(cGame pownergame) : base(pownergame)
        {

            int ranOluveColor = Framework.models.selectRandomOluve(5, 10);

            //determines which random oluve is chosen and is drawn, room will be filled with all random colored ones
            if (ranOluveColor == 5)
            {
                Sprite = new cSpriteQuake(ModelsMD2.oluvegold);
                setRadius(0.8f);
            }
            else if (ranOluveColor == 6)
            {
                Sprite = new cSpriteQuake(ModelsMD2.oluvegray);
                setRadius(0.8f);
            }
            else if (ranOluveColor == 7)
            {
                Sprite = new cSpriteQuake(ModelsMD2.oluvegreen);
                setRadius(0.8f);
            }
            else if (ranOluveColor == 8)
            {
                Sprite = new cSpriteQuake(ModelsMD2.oluvered);
                setRadius(0.8f);
            }
            else if (ranOluveColor == 9)
            {
                Sprite = new cSpriteQuake(ModelsMD2.oluvesilver);
                setRadius(0.8f);
            }
            else if (ranOluveColor == 10)
            {
                Sprite = new cSpriteQuake(ModelsMD2.oluvewhite);
                setRadius(0.8f);
            }

            Sprite.setstate(State.Other, 6, 37, StateType.Repeat); //turn
            //Sprite.setstate(State.Other, 94, 111, StateType.Repeat); //spin

            Bounciness = 2.0f; //maximum bounciness

            MaxSpeed = 30.0f; //max speed of the critters
            //the higher the number the more bouncing will occur 

            //randomizes starting position
            randomizePosition(new cRealBox3(new cVector3(_movebox.Lox, _movebox.Loy, _movebox.Loz + 4.0f),
                new cVector3(_movebox.Hix, _movebox.Loy, _movebox.Midz - 1.0f)));

            //no friction for the bouncy balls so that they just keep moving
            addForce(new cForceDrag(0.0f));  

            //makes them bounce
            _wrapflag = cCritter.BOUNCE;
        }
        public override bool IsKindOf(string str)
        {
            return str == "cEnemyOluve" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cEnemyOluve";
            }
        }
    }

    /*'die' (disappear) function*/
    class cEnemyPenquin : cCritter3Dcharacter
    {
        public cEnemyPenquin(cGame pownergame) : base(pownergame)
        {
            
            Sprite = new cSpriteQuake(ModelsMD2.penguin);
            Sprite.setstate(State.Other, 135, 172, StateType.Repeat); //sliding

            Framework.snd.play(Sound.Penguin1);

            Speed = 30.0f;
            Bounciness = 0.5f;
            _wrapflag = cCritter.BOUNCE;
            addForce(new cForceDrag(0.0f));

            
        }
        public override bool IsKindOf(string str)
        {
            return str == "cEnemyPenquin" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cEnemyPenquin";
            }
        }
    }

    /*add more states?*/
    class cEnemyPotator : cCritter3Dcharacter
    {
        
        public cEnemyPotator(cGame pownergame) : base(pownergame) 
        {
            
            Sprite = new cSpriteQuake(ModelsMD2.potator);
            //Sprite.setstate(State.Other, 3, 27, StateType.Repeat); //picture
            //Sprite.setstate(State.Other, 45, 50, StateType.Repeat); //laughing
            randomizeVelocity(0, 0, false);
            addForce(new cForceDrag(20.0f));

            setRadius(1.5f);

            int positionCount = 0;
            cVector3[] positions = new cVector3[1]; //to store the positions of the hand guns
            Attitude = new cMatrix3(new cVector3(0.0f, 0.0f, 1.0f), new cVector3(1.0f, 0.0f, 0.0f), new cVector3(0.0f, 1.0f, 0.0f), Position);
            positions[0] = new cVector3(0, -8, 0);
            moveTo(positions[positionCount]);
        }
 
       
        public override bool IsKindOf(string str)
        {
            return str == "cEnemyPotator" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cEnemyPotator";
            }
        }
    }

    class cEnemyPotatorWin : cEnemyPotator
    {
        public cEnemyPotatorWin(cGame pownergame) : base(pownergame)
        {
            Sprite.setstate(State.Other, 3, 27, StateType.Repeat); //picture
        }
    }
    class cEnemyPotatorLose : cEnemyPotator
    {
        public cEnemyPotatorLose(cGame pownergame) : base(pownergame)
        {
            Sprite.setstate(State.Other, 45, 50, StateType.Repeat); //laughing
        }
    }
 	
	//======================cGame3D========================== 
	
	class cGame3D : cGame 
	{ 
		public static readonly float WALLTHICKNESS = 0.5f; 
		public static readonly float PLAYERRADIUS = 0.2f; 
		public static readonly float MAXPLAYERSPEED = 25.0f;
		private bool doorcollision;
        private bool wentThrough = false;
        private float startNewRoom;
        private int roomNumber = 0; //Tracks the room we are in 

        //------------Rooms----------

		public cGame3D() 
		{
			doorcollision = false; 
			_menuflags &= ~ cGame.MENU_BOUNCEWRAP; 
			_menuflags |= cGame.MENU_HOPPER; //Turn on hopper listener option.
			_spritetype = cGame.ST_MESHSKIN; 
			setBorder( 64.0f, 16.0f, 64.0f ); // size of the world
		
			cRealBox3 skeleton = new cRealBox3();
            skeleton.copy(_border);
			setSkyBox( skeleton );
            
            SkyBox.setSideTexture(cRealBox3.HIZ, BitmapRes.snowWall); //Make the near HIZ transparent
            SkyBox.setSideTexture(cRealBox3.LOZ, BitmapRes.snowWall); //Far wall
            SkyBox.setSideTexture(cRealBox3.LOX, BitmapRes.snowWall); //left wall
            SkyBox.setSideTexture( cRealBox3.HIX, BitmapRes.snowWall); //right wall
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.iceFloor ); //floor
			SkyBox.setSideTexture( cRealBox3.HIY, BitmapRes.cloudySky ); //ceiling
		
			WrapFlag = cCritter.BOUNCE;
			_seedcount = 20; 
			setPlayer( new cCritter3DPlayer( this ));
            cEnemyJack mrFrost1 = new cEnemyJack(this, new cVector3(_border.Hix, _border.Hiy, _border.Hiz));
            
           
			cCritterDoor pdwall = new cCritterDoor( 
				new cVector3( _border.Lox, _border.Loy, _border.Midz ), 
				new cVector3( _border.Lox, _border.Midy - 3, _border.Midz ), 
				0.1f, 2, this ); 
            
			cSpriteTextureBox pspritedoor = new cSpriteTextureBox( pdwall.Skeleton, BitmapRes.Door, 1); 
			pdwall.Sprite = pspritedoor;
        }
             
        public void setBouncyBallRoom()
        {
            Biota.purgeCritters("cCritterWall"); //copy these 2 lines
            Biota.purgeCritters("cCritter3Dcharacter");

            setBorder(50.0f, 15.0f, 50.0f); //the dimensions of the room (room length, ceiling height, room width)

            cRealBox3 skeleton = new cRealBox3(); //just copy these 3 lines
            skeleton.copy(_border);
            setSkyBox(skeleton);

            SkyBox.setAllSidesTexture(BitmapRes.bounceWall, 1); //wall bitmap
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.bounceWall); //floor bitmap
            SkyBox.setSideTexture(cRealBox3.HIY, BitmapRes.bounceWall); //ceiling bitmap

            Player.setMoveBox(new cRealBox3(50.0f, 15.0f, 50.0f)); //make the same as the border of the room

            _seedcount = 30; //the more there are the more bounciness occurs

            seedCritters(); //has to be called again so that the oluves are drawn

            wentThrough = true; //copy these 2 lines
            startNewRoom = Age;

            //all of this following code is to create the door and the location of the door
            //already have the door set to be directly across on the other side
            //change x values for positions on walls
            //change y valuse to change the way the door is fixed on that particular wall
            cCritterDoor pdwall = new cCritterDoor(
                new cVector3(_border.Lox, _border.Loy, _border.Midz),
                new cVector3(_border.Lox, _border.Midy - 3, _border.Midz),
                0.1f, 2, this);
            cSpriteTextureBox pspritedoor = // change this variable name to detemrine collisions with this specific door
                new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door);
            pdwall.Sprite = pspritedoor;
        }

        public void setHandgunRoom()
        {
            Biota.purgeCritters("cCritterWall"); //copy these 2 lines
            Biota.purgeCritters("cCritter3Dcharacter");
             
            setBorder(30.0f, 15.0f, 10.0f); //the dimensions of the room (room length, ceiling height, room width)
            
            cRealBox3 skeleton = new cRealBox3(); //just copy these 3 lines
            skeleton.copy(_border);
            setSkyBox(skeleton);

            SkyBox.setAllSidesTexture(BitmapRes.fireWall, 1); //wall bitmap
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.metalFloor); //floor bitmap
            SkyBox.setSideTexture(cRealBox3.HIY, BitmapRes.metalFloor); //ceiling bitmap

            Player.setMoveBox(new cRealBox3(30.0f, 15.0f, 10.0f)); //make the same as the border of the room
            


            _seedcount = 1;
            seedCritters();

            wentThrough = true; //copy these 2 lines
            startNewRoom = Age;

            float height = _border.YSize;
            float ycenter = -_border.YRadius + height / 2;
            float wallthickness = cGame3D.WALLTHICKNESS;

            cCritterWallMover pwall;
            pwall = new cCritterWallMover(new cVector3(_border.Lox +.2f,  ycenter, _border.Midz),
                                          new cVector3(_border.Lox +.2f, ycenter, _border.Midz),
                                          height, wallthickness, this);
            cSpriteTextureBox pspritebox;
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.blackCeiling, 1);
            pwall.Sprite = pspritebox;

            Player.moveTo(new cVector3(14, -8, 0));

            //all of this following code is to create the door and the location of the door
            cCritterDoor pdwall = new cCritterDoor(
                new cVector3(_border.Hix, _border.Loy, _border.Midz),
                new cVector3(_border.Hix, _border.Midy - 3, _border.Midz),
                0.1f, 2, this);
            cSpriteTextureBox pspritedoor = // change this variable name to detemrine collisions with this specific door
                new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door);
            pdwall.Sprite = pspritedoor;
        }

        public void setMazeRoom() ///make some grass and shit
        {
            Biota.purgeCritters("cCritterWall"); //copy these 2 lines
            Biota.purgeCritters("cCritter3Dcharacter");
            Biota.purgeNonPlayerCritters();

            Player.moveTo(new cVector3(_border.Hix, _border.Loy, _border.Midz));
            

            setBorder(50.0f, 15.0f, 50.0f); //the dimensions of the room (room length, ceiling height, room width)

            cRealBox3 skeleton = new cRealBox3(); //just copy these 3 lines
            skeleton.copy(_border);
            setSkyBox(skeleton);

            SkyBox.setAllSidesTexture(BitmapRes.stoneWall); //wall bitmap
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.grassFloor); //floor bitmap
            SkyBox.setSideTexture(cRealBox3.HIY, BitmapRes.cloudySky); //ceiling bitmap

            Player.setMoveBox(new cRealBox3(50.0f, 15.0f, 50.0f)); //make the same as the border of the room

            _seedcount = 0;

            wentThrough = true; //copy these 2 lines
            startNewRoom = Age;

            float height = _border.YSize / 10;
            float ycenter = -_border.YRadius + height / 2;
            float wallthickness = cGame3D.WALLTHICKNESS;
            cCritterWall pwall1 = new cCritterWall(new cVector3(_border.Lox, ycenter, _border.Loz + 30),
                                                  new cVector3(_border.Hix, ycenter, _border.Loz + 30),
                                                  height * 10,
                                                  wallthickness,
                                                  this);
            cSpriteTextureBox pspritebox = new cSpriteTextureBox(pwall1.Skeleton, BitmapRes.stoneWall);
            pwall1.Sprite = pspritebox;
            cCritterWall pwall2 = new cCritterWall(new cVector3(_border.Lox, ycenter, _border.Loz + 20),
                                                  new cVector3(_border.Hix, ycenter, _border.Loz + 20),
                                                  height * 10,
                                                  wallthickness,
                                                  this);
            cSpriteTextureBox pspritebox2 = new cSpriteTextureBox(pwall2.Skeleton, BitmapRes.stoneWall);
            pwall2.Sprite = pspritebox2;

            cCritterWall pwall3map = new cCritterWall(new cVector3(_border.Hix, _border.Loy, _border.Midz),
                                                      new cVector3(_border.Hix, _border.Midy, _border.Midz),
                                                         0.1f,
                                                         8, this);
            cSpriteTextureBox pspritebox3map = new cSpriteTextureBox(pwall3map.Skeleton, BitmapRes.map);
            pwall3map.Sprite = pspritebox3map;

            //all of this following code is to create the door and the location of the door
            //already have the door set to be directly across on the other side
            //change x values for positions on walls
            //change y valuse to change the way the door is fixed on that particular wall
            cCritterDoor pdwall = new cCritterDoor(
                new cVector3(_border.Lox, _border.Loy, _border.Midz),
                new cVector3(_border.Lox, _border.Midy - 3, _border.Midz),
                0.1f, 2, this);
            cSpriteTextureBox pspritedoor = // change this variable name to detemrine collisions with this specific door
                new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door);
            pdwall.Sprite = pspritedoor;
        }

        public void setWinRoom()
        {
            Biota.purgeCritters("cCritterWall"); //copy these 2 lines
            Biota.purgeCritters("cCritter3Dcharacter");

            setBorder(15.0f, 15.0f, 15.0f); //the dimensions of the room (room length, ceiling height, room width)

            cRealBox3 skeleton = new cRealBox3(); //just copy these 3 lines
            skeleton.copy(_border);
            setSkyBox(skeleton);

            SkyBox.setAllSidesTexture(BitmapRes.cloudySky, 1); //wall bitmap
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.cloudySky); //floor bitmap
            SkyBox.setSideTexture(cRealBox3.HIY, BitmapRes.cloudySky); //ceiling bitmap

            Player.setMoveBox(new cRealBox3(15.0f, 15.0f, 15.0f)); //make the same as the border of the room

            _seedcount = 1; 

            seedCritters(); 

            wentThrough = true; //copy these 2 lines
            startNewRoom = Age;
        }

        public void setLoseRoom()
        {
            Biota.purgeCritters("cCritterWall"); //copy these 2 lines
            Biota.purgeCritters("cCritter3Dcharacter");

            setBorder(15.0f, 15.0f, 15.0f); //the dimensions of the room (room length, ceiling height, room width)

            cRealBox3 skeleton = new cRealBox3(); //just copy these 3 lines
            skeleton.copy(_border);
            setSkyBox(skeleton);

            SkyBox.setAllSidesTexture(BitmapRes.blackCeiling, 1); //wall bitmap
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.blackCeiling); //floor bitmap
            SkyBox.setSideTexture(cRealBox3.HIY, BitmapRes.blackCeiling); //ceiling bitmap

            Player.setMoveBox(new cRealBox3(15.0f, 15.0f, 15.0f)); //make the same as the border of the room

            _seedcount = 1;

            seedCritters();
            Framework.snd.play(Sound.Laugh);
            wentThrough = true; //copy these 2 lines
            startNewRoom = Age;
        }


        //------------- End of rooms -----------
        public override void seedCritters()
		{
			Biota.purgeCritters( "cCritterBullet" ); 
			Biota.purgeCritters( "cCritter3Dcharacter" );

            if(roomNumber == 0)
            {
                for (int i = 0; i < _seedcount; i++)
                    new cEnemyPenquin(this);
                for (int i = 0; i < _seedcount/2; i++)
                    new cEnemyMrFrost(this);
            }
            if (roomNumber == 1)
            {
                for (int i = 0; i < _seedcount; i++)
                    new cEnemyOluve(this);
            }
            if(roomNumber == 2)
            {
                for (int i = 0; i < _seedcount; i++)
                    new cEnemyHand(this);
            }
            if (roomNumber == 3)
            {
                for (int i = 0; i < _seedcount; i++)
                    new cEnemyPotatorWin(this);
            }
            if(roomNumber == 4)
            {
                for (int i = 0; i < _seedcount; i++)
                    new cEnemyPotatorLose(this);
            }

            Player.moveTo(new cVector3(0.0f, Border.Loy, Border.Hiz - 3.0f)); 
				/* We start at hiz and move towards	loz */ 
		} 

		public void setdoorcollision( ) { doorcollision = true; } 
		
		public override ACView View 
		{
            set
            {
                base.View = value; //You MUST call the base class method here.
                value.setUseBackground(ACView.FULL_BACKGROUND); /* The background type can be
			    ACView.NO_BACKGROUND, ACView.SIMPLIFIED_BACKGROUND, or 
			    ACView.FULL_BACKGROUND, which often means: nothing, lines, or
			    planes&bitmaps, depending on how the skybox is defined. */
                value.pviewpointcritter().Listener = new cListenerViewerRide();
            }
		} 

		public override cCritterViewer Viewpoint 
		{ 
            set
            {
			    if ( value.Listener.RuntimeClass == "cListenerViewerRide" ) 
			    { 
				    value.setViewpoint( new cVector3( 0.0f, 0.3f, -1.0f ), _border.Center); 
					//Always make some setViewpoint call simply to put in a default zoom.
				    value.zoom( 0.35f ); //Wideangle 
				    cListenerViewerRide prider = ( cListenerViewerRide )( value.Listener); 
				    prider.Offset = (new cVector3( -1.5f, 0.0f, 1.0f)); /* This offset is in the coordinate
				    system of the player, where the negative X axis is the negative of the
				    player's tangent direction, which means stand right behind the player. */ 
			    } 
			    else //Not riding the player.
			    { 
				    value.zoom( 1.0f ); 
				    /* The two args to setViewpoint are (directiontoviewer, lookatpoint).
				    Note that directiontoviewer points FROM the origin TOWARDS the viewer. */ 
				    value.setViewpoint( new cVector3( 0.0f, 0.3f, 1.0f ), _border.Center); 
			    }
            }
		} 

	
	
		public override void adjustGameParameters() 
		{
		// (1) End the game if the player is dead 
			/**if ( (Health == 0) && !_gameover ) //Player's been killed and game's not over.
			{ 
				_gameover = true; 
				Player.addScore( _scorecorrection ); // So user can reach _maxscore  
                Framework.snd.play(Sound.Hallelujah);
                return ; 
			} 


            //game doesn't end on it's own

            
            //commented this out so that the critters do not come back, once they are gone, they are gone for good

		// (2) Also don't let the the model count diminish.
					//(need to recheck propcount in case we just called seedCritters).
			//int modelcount = Biota.count( "cCritter3Dcharacter" ); 
			//int modelstoadd = _seedcount - modelcount; 
			//for ( int i = 0; i < modelstoadd; i++) 
			//	new cCritter3Dcharacter( this ); 

		// (3) Maybe check some other conditions.*/

            if (wentThrough && (Age - startNewRoom) > 2.0f)
            {
                wentThrough = false;
            }

            if (doorcollision == true)
            {
                if (roomNumber == 0)
                {
                    roomNumber = 1;
                    setBouncyBallRoom();
                }
                else if (roomNumber == 1)
                {
                    roomNumber = 2;
                    setHandgunRoom();
                }
                else if (roomNumber == 2)
                {
                    roomNumber = 3;
                    setMazeRoom();
                }
                else if (roomNumber == 3)
                {
                    setWinRoom();
                }
                doorcollision = false;                
            }

            if(cCritterPlayer._loseRoom == true)
            {
                roomNumber = 4;
                setLoseRoom();
                cCritterPlayer._loseRoom = false;
            }
		}
		
	} 
	
}