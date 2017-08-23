#! /usr/bin/python
from sys import argv, exit
import datetime
from re import *
from numpy import *

try:
  from OpenGL.GLUT import *
  from OpenGL.GL import *
  from OpenGL.GLU import *
except:
  print ''' Fehler: PyOpenGL not installed properly !!'''
  sys.exit(  )

FLIPVIEW = True

from stereoCamera import StereoCamera
sC = StereoCamera( )

movingAtomNewCoordsList = []
interpolatedMovingAtomCoordsList = []
movingAtomNewCoordsList = []
maxfV = 0
previousIndex = 0
interpolationsFaktor = 100
bewegungsDauer = 0

ESCAPE = '\033'

scalarFieldDimensions = []
scalarField = []
scalarFieldA = ([[]])

animationAngle = 0.0
frameRate = 120
stereoMode = "NONE"
lightColors = {
	"white":(1.0, 1.0, 1.0, 1.0),
	"red":(1.0, 0.0, 0.0, 1.0),
	"green":(0.0, 1.0, 0.0, 1.0),
	"blue":(0.0, 0.0, 1.0, 1.0)
}

lightPosition = (5.0, 5.0, 20.0, 1.0)

from time import sleep
		
def animationStep( ):
	"""Update animated parameters."""
	global animationAngle
	global frameRate
	animationAngle += 0 # 2
	while animationAngle > 360:
		animationAngle -= 360
	sleep( 1 / float( frameRate ) )
	glutPostRedisplay( )
	### explicitly demands that window content is redrawn

def setLightColor( s ):
	"""Set light color to 'white', 'red', 'green' or 'blue'."""
	if lightColors.has_key( s ):
		c = lightColors[ s ]
		glLightfv( GL_LIGHT0, GL_AMBIENT, c )
		glLightfv( GL_LIGHT0, GL_DIFFUSE, c )
		glLightfv( GL_LIGHT0, GL_SPECULAR, c )

def render( side ):
	global FLIPVIEW
	#global previousCoordX
	#global previousCoordY
	#global previousCoordZ 
	global previousIndex
	global maxfV
	global bewegungsDauer
	global time_offset
	global freeze
	global index_freeze
	global index
	global mod_time
	global release_time
	global radius
	global newCoordsList
	global yShift
	global zShift
	global showEnergy
	#global xcam
	#global ycam
	#global zcam
	
	"""Render scene in either GLU_BACK_LEFT or GLU_BACK_RIGHT buffer"""
	glViewport( 0, 0,
		glutGet( GLUT_WINDOW_WIDTH ), glutGet( GLUT_WINDOW_HEIGHT ))
	if side == GL_BACK_LEFT:
		if FLIPVIEW:
			f = sC.frustumRight
			l = sC.lookAtRight
		else:
			f = sC.frustumLeft
			l = sC.lookAtLeft
	else:
		if FLIPVIEW:
			f = sC.frustumLeft
			l = sC.lookAtLeft
		else:
			f = sC.frustumRight
			l = sC.lookAtRight
		
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glFrustum( f[0], f[1], f[2], f[3], f[4], f[5] )
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
	gluLookAt( l[0], l[1], l[2], l[3], l[4], l[5], l[6], l[7], l[8] )
	
	glRotatef( angleX, 1.0, 0.0, 0.0 )	
	glRotatef( angleY, 0.0, 1.0, 0.0 )
	glRotatef( angleZ, 0.0, 0.0, 1.0 )
	
	glTranslate(xcam, ycam, zcam)
	
	# stationary objects
	#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.2, 0.2, 0.2, 1] )
	#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0., 0., 0., 1] )
	#glCallList( teapotList )
	#glCallList( fieldDataList)
	
	if showEnergy:
		glCallList( fieldDataList)
	else:
		pass

	
	glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.0, 0., 0.2, 0.1] )
	for i in newCoordsList:
		glTranslate(i[0], i[1], i[2])
		
		glutSolidSphere(radius,16,16)
		glTranslate(-i[0], -i[1], -i[2])
		
		glTranslate(i[0], i[1]+yShift, i[2])
		
		glutSolidSphere(radius,16,16)
		glTranslate(-i[0], -(i[1]+yShift), -i[2])
		
		
		
		glTranslate(i[0], i[1], i[2]+zShift)
		
		glutSolidSphere(radius,16,16)
		glTranslate(-i[0], -i[1], -(i[2]+zShift))
		
		glTranslate(i[0], i[1]+yShift, i[2]+zShift)
		
		glutSolidSphere(radius,16,16)
		glTranslate(-i[0], -(i[1]+yShift), -(i[2]+zShift))
	
	
	
	
	# objects that change in time
	now         = datetime.datetime.now()
	micro       = now.microsecond
	hour        = now.hour
	minute      = now.minute
	second      = now.second
	
	if previousIndex == int( (minute*60+second)%len(movingAtomNewCoordsList) ):
		sameIndex = 1
	else:
		sameIndex = 0
	
	our_time = 7.*(hour*3600+minute*60+second+micro/1000000.0 - time_offset) 
	
	mod_time = int(our_time)
	
	if (freeze==1):
	  index = index_freeze
	else:	
		index = (mod_time - release_time + index_freeze) % bewegungsDauer
	
	###### part for moving H Atom
	#currentCoordOfMovingAtom = movingAtomNewCoordsList[int( (minute*60+second)%len(movingAtomNewCoordsList) )]
	currentCoordOfMovingAtom = movingAtomNewCoordsList[index]
	currentCoordX = currentCoordOfMovingAtom[0]
	currentCoordY = currentCoordOfMovingAtom[1]
	currentCoordZ = currentCoordOfMovingAtom[2]
	currentCoordE = currentCoordOfMovingAtom[3]
	
	previousIndex = int( (minute*60+second)%len(movingAtomNewCoordsList) )
	
	if sameIndex == 0:
		pass
		#print minute*60+second, int( (minute*60+second)%len(movingAtomNewCoordsList) ), ' minute*60+second, int( (minute*60+second)%len(movingAtomNewCoordsList) ) ' 
		#print int( (minute*60+second)%len(movingAtomNewCoordsList) ), previousIndex, ' int( (minute*60+second)%len(movingAtomNewCoordsList) ), previousIndex '
		#print currentCoordX, currentCoordY, currentCoordZ, ' X, Y, Z' 
	else:
		pass
	
	glTranslate(currentCoordX, currentCoordY, currentCoordZ)
	
	glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.2, 0., 0.,1.0] )	### sets now only for the next object 
	glutSolidSphere(0.05,16,16)	
	
	glTranslate(-currentCoordX, -currentCoordY, -currentCoordZ)
	
	#traceIndex = int( (minute*60+second)%len(movingAtomNewCoordsList) )
	traceIndex = index
	
	#for el in movingAtomNewCoordsList:
	#	if (float( el[3] )/maxfV) > 0.5:
	#		print float( el[3] )/maxfV
	#	print '#####'
	#exit(1)
	#maxVal = 0.
	#for el in movingAtomNewCoordsList:
	#	if float(el[3]) >= maxVal:
	#		print float(el[3])
	#		maxVal = float(el[3])
	#	else:
	#		pass
	#print maxVal
	#print maxfV
	#exit(1)
	
	#glDisable(GL_LIGHT0)		
							
	for i in range(traceIndex):
		rVal = (float( movingAtomNewCoordsList[i][3] )) 
		glTranslate(movingAtomNewCoordsList[i][0], movingAtomNewCoordsList[i][1], movingAtomNewCoordsList[i][2])
		#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [(float( movingAtomNewCoordsList[i][3] )/maxfV), (1-float( movingAtomNewCoordsList[i][3] )/maxfV) , (1-float( movingAtomNewCoordsList[i][3] )/maxfV) ,1.0] )
		#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [1-exp(-2.*rVal), .9*exp(2.*(-4.*(rVal-0.5)*(rVal-0.5))), .75*exp(-2.*(1-rVal))  ,1.0] )
		#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [1, 0, 0  ,1.0] )
		#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.1 + ( 0.9*(float( movingAtomNewCoordsList[i][3] )) ), 0.25 - ( 0.25*(float( movingAtomNewCoordsList[i][3] ))*(float( movingAtomNewCoordsList[i][3] ))*(float( movingAtomNewCoordsList[i][3] )) ), 0. ,1.0] )
		glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rVal*rVal ,  1.*(0.5-rVal)*(0.5-rVal), (1-rVal)*(1-rVal) ,0.4]  )
		#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rVal ,  1.*(0.5-rVal)*(0.5-rVal), (1-rVal)*(1-rVal) ,0.4]  )
		#if (rVal >= 6./7.):
		#	RGB_R = 1.
		#	RGB_G = rVal
		'''
		#if ((0.75 <= (float(movingAtomNewCoordsList[i][3])/maxfV)) & (1.0 > (float( movingAtomNewCoordsList[i][3] )/maxfV) ) ):
		if 0.75 <= rVal:
			glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [255, 255*(1- 4.*(rVal - 0.75) ) , 0. ,1.0] )
			print rVal , ' >0.75 ', 255*(1- 4.*(rVal - 0.75) ) 
		#elif ( (0.5 <= (float( movingAtomNewCoordsList[i][3] )/maxfV)) & (0.75 > (float( movingAtomNewCoordsList[i][3] )/maxfV) ) ):
		elif 0.5 <= rVal:
			glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [255*(1.- 4*(0.75 - rVal)), 255, 0. ,1.0] )
			print rVal, ' >0.5 ', 255*(1.- 4*(0.75 - rVal))
		#elif ( (0.25 <= float( movingAtomNewCoordsList[i][3] )/maxfV) & (0.5 > float( movingAtomNewCoordsList[i][3] )/maxfV) ):
		elif 0.25 <= rVal: 
			glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0, 255, 255*(1.- 4*(rVal - 0.25)) ,1.0] )	
			print rVal , ' >0.25 ', 255*(1.- 4*(rVal - 0.25))
		#elif ( (0. <= float( movingAtomNewCoordsList[i][3] )/maxfV) & (0.25 > float( movingAtomNewCoordsList[i][3] )/maxfV) ):
		else:
			glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0, 255*(1.-4.*(0.25 - rVal)), 255 ,1.0] )
			print rVal, ' <0.25 ' ,255*(1.-4.*(0.25 - rVal))
		'''	
			
		glutSolidSphere(0.005,16,16)	
		glTranslate(-(movingAtomNewCoordsList[i][0]), -movingAtomNewCoordsList[i][1], -(movingAtomNewCoordsList[i][2]))
		#print movingAtomNewCoordsList[i][2]
	
	
	glTranslate(movingAtomNewCoordsList[0][0], movingAtomNewCoordsList[0][1], movingAtomNewCoordsList[0][2])
	rValBegin = (float( movingAtomNewCoordsList[0][3] ))
	#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [1-exp(-2.*rValBegin), .9*exp(2.*(-4.*(rValBegin-0.5)*(rValBegin-0.5))), .75*exp(-2.*(1-rValBegin))  ,1.0] )
	#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.1 + ( 0.9*(float( movingAtomNewCoordsList[0][3] )) ), 0.25 - ( 0.25*(float( movingAtomNewCoordsList[0][3] ))*(float( movingAtomNewCoordsList[0][3] ))*(float( movingAtomNewCoordsList[0][3] )) ), 0. ,1.0] )
	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rValBegin*rValBegin ,  1.*(0.5-rValBegin)*(0.5-rValBegin), (1-rValBegin)*(1-rValBegin) ,0.4]  )
	#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rValBegin ,  1.*(0.5-rValBegin)*(0.5-rValBegin), (1-rValBegin)*(1-rValBegin) ,0.4]  )
	glutSolidSphere(0.025,16,16)
	glTranslate(-(movingAtomNewCoordsList[0][0]), -movingAtomNewCoordsList[0][1], -(movingAtomNewCoordsList[0][2]))

	
	glTranslate(movingAtomNewCoordsList[-1][0], movingAtomNewCoordsList[-1][1], movingAtomNewCoordsList[-1][2])
	rValEnd = (float( movingAtomNewCoordsList[-1][3] )) 
	#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [1-exp(-2.*rValEnd), .9*exp(2.*(-4.*(rValEnd-0.5)*(rValEnd-0.5))), .75*exp(-2.*(1-rValEnd))  ,1.0] )
	#glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.1 + ( 0.9*(float( movingAtomNewCoordsList[-1][3] )) ), 0.25 - ( 0.25*(float( movingAtomNewCoordsList[-1][3] ))*(float( movingAtomNewCoordsList[-1][3] ))*(float( movingAtomNewCoordsList[-1][3] )) ), 0. ,1.0] )
	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rValEnd*rValEnd ,  1.*(0.5-rValEnd)*(0.5-rValEnd), (1-rValEnd)*(1-rValEnd) ,0.4]  )
	#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rValEnd ,  1.*(0.5-rValEnd)*(0.5-rValEnd), (1-rValEnd)*(1-rValEnd) ,0.4]  )
	glutSolidSphere(0.025,16,16)
	glTranslate(-(movingAtomNewCoordsList[-1][0]), -movingAtomNewCoordsList[-1][1], -(movingAtomNewCoordsList[-1][2]))
	
	#glEnable(GL_LIGHT0)
	
	#glTranslate(xcam, ycam, zcam)
	
	#glRotatef( angleX, 1.0, 0.0, 0.0 )	
	#glRotatef( angleY, 0.0, 1.0, 0.0 )
	#glRotatef( angleZ, 0.0, 0.0, 1.0 )
	
	#drawText("xcam="+str("%.2f" % xcam)+" ycam="+str("%.2f" % ycam)+" zcam="+str("%.2f" % zcam), 10, 30, 500, 500)
	#drawText("angleX="+str("%.2f" % angleX)+" angleY="+str("%.2f" % angleY)+" angleZ="+str("%.2f" % angleZ), 10, 20, 500, 500)

def drawText( value, x,y,  windowHeight, windowWidth, step = 18 ):
	glMatrixMode(GL_PROJECTION);
	
	# For some reason the GL_PROJECTION_MATRIX is overflowing with a single push!
	
	# glPushMatrix()
	
	matrix = glGetDouble( GL_PROJECTION_MATRIX )
	glLoadIdentity();		
	glOrtho(0.0, windowHeight or 32, 0.0, windowWidth or 32, -1.0, 1.0)
	glMatrixMode(GL_MODELVIEW);
	glPushMatrix();
	glLoadIdentity();
	glRasterPos2i(x, y);
	lines = 0
	
	for character in value:
		if character == '\n':
			glRasterPos2i(x, y-(lines*18))
		else:
			glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, ord(character));
	glPopMatrix();
	glMatrixMode(GL_PROJECTION);
	glLoadMatrixd( matrix ) # should have un-decorated alias for this...
	glMatrixMode(GL_MODELVIEW);
	
def display(  ):
### is called when window content must be redrawn 
	"""Glut display function."""
    
	if stereoMode != "SHUTTER":
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

	if stereoMode == "SHUTTER":
		setLightColor( "white" )
		glDrawBuffer( GL_BACK_LEFT )
		glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT )
		render( GL_BACK_LEFT )
		glDrawBuffer( GL_BACK_RIGHT )
		glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT )
		render( GL_BACK_RIGHT )
	elif stereoMode == "ANAGLYPH":
		glDrawBuffer( GL_BACK_LEFT )
		glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT )
		setLightColor( "red" )
		render( GL_BACK_LEFT )
		glClear( GL_DEPTH_BUFFER_BIT )
		glColorMask( False, True, False, False )
		setLightColor( "green" )
		render( GL_BACK_RIGHT )
		glColorMask( True, True, True, True )
	else:
		glDrawBuffer(GL_BACK_LEFT)
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
		setLightColor( "white" )
		render(GL_BACK_LEFT)
	glutSwapBuffers( )

def UpdateCamera():
	global sC
	global basis
	
	global xcam, ycam, zcam
	global angleY
	global angleZ
	global angleX
	
	sC.aperture = 40.0
	sC.focalLength = 10.0
	sC.centerPosition[0], sC.centerPosition[1], sC.centerPosition[2] = 0.0, 0.0, 5.0
	sC.viewingDirection[0], sC.viewingDirection[1], sC.viewingDirection[2] = 0.0, 0.0, -1.0
	#sC.centerPosition[0], sC.centerPosition[1], sC.centerPosition[2] = xcam, ycam, zcam
	
	#sC.viewingDirection[0], sC.viewingDirection[1], sC.viewingDirection[2] = -1.0*sin(angleY)*cos(angleX), sin(angleX), -1.0*cos(angleY)*cos(angleX)	
		
	sC.near = sC.focalLength / 500.0
	sC.far = 1000
	sC.eyeSeparation = sC.focalLength / basis
	sC.whRatio = float( glutGet( GLUT_WINDOW_WIDTH ) ) /  glutGet( GLUT_WINDOW_HEIGHT )
	sC.update( )


def pbc_1(var):
	if var-1 > 0:
		return var-1
	elif var-1 < 0:
		return var
	else:
		print 'error in periodic bc shift for coordinates'
		exit(1)
####################################################################################
def init(  ):
	global scalarField
	global scalarFieldDimensions 
	global maxfV
	global fieldDataList
	global teapotList
	global time_offset
	global bewegungsDauer
	global newCoordsList
	global yShift
	global zShift
	

	try:
		re_n = compile('\n') ### re includes only break 
		re_whitespace = compile('\s')  ### re includes whitespaces \t \n \r \f \v
	except:
		print ''' could not define regular expression '''
	
	try:
		datenPES = file('PES_Sigma11', 'r')
		datenPESListe = datenPES.readlines()
		print datenPESListe[2], datenPESListe[3], datenPESListe[4]
		
		print float(datenPESListe[3].split()[1])
	
		unitVector = [float(datenPESListe[2].split()[0]), float(datenPESListe[3].split()[1]), float(datenPESListe[4].split()[2])]
		
		print unitVector
		
		#exit(1)
		datenFixedGrid3D = file('atomPositionsSigma11.txt', 'r')
		datenFixedGrid3DListe = datenFixedGrid3D.readlines()
		laenge = len(datenFixedGrid3DListe)
		
		datenMovingAtom3D = file('hydrogenPathS11.txt','r') ### is time sequence of position vectors of 
		### moving H Atom
		### only for test take datenFixedGrid3D.txt and divide values by 10 for smoother walk
		datenMovingAtom3DListe = datenMovingAtom3D.readlines()
		bewegungsDauer = len(datenMovingAtom3DListe)
		
		fieldData = file('fieldData_180x26x64S11.txt', 'r')
		fieldDataList = fieldData.readlines()
		fieldDataAmount = len(fieldDataList)
		 
		scalarFieldDimensions = fieldDataList[0].split()
		fieldDataList.pop(0)
		print int(scalarFieldDimensions[0]), int(scalarFieldDimensions[1]), int(scalarFieldDimensions[2])
		
		#scalarFieldA = array()
		
		counter = 0
		for xIndex in range(int(scalarFieldDimensions[2])):
			for yIndex in range(int(scalarFieldDimensions[1])):
				for zIndex in range(int(scalarFieldDimensions[0])):
					scalarField.append([xIndex,yIndex,zIndex,float(fieldDataList[counter])+0.1])# = fieldDataList[counter]+0.1
					counter = counter + 1
		
			
	except:
		print ''' Cannot read/print daten3D.txt / datenMovingAtom3D.txt '''
	
	newCoordsList = []
	
	for i in range(laenge):
		coord = re_whitespace.split(datenFixedGrid3DListe[i])
			
		xCoord = pbc_1(float(coord[0]))
		yCoord = pbc_1(float(coord[1]))
		zCoord = pbc_1(float(coord[2]))			
				
		scaleUp = 4.
		
		xStretch = 1.#float(1./float(unitVector[0]))
		yStretch = float(unitVector[1]/unitVector[0])#*xStretch
		zStretch = float(unitVector[2]/unitVector[0])#*xStretch
		
		yShift = scaleUp*yStretch
		zShift = -scaleUp*zStretch
		#print yStretch, zStretch, 'y- and zStretch'
						
		newCoord = [xCoord*scaleUp,yCoord*yStretch*scaleUp,zCoord*zStretch*scaleUp]			
		newCoordsList.append(newCoord)	
	
	for i in range(bewegungsDauer):
		#'www.example.com'.strip('cmowz.')
		el = datenMovingAtom3DListe[i].split(',')
					
		movingAtomXCoord = 1.*scaleUp*(float(el[0]))/float(scalarFieldDimensions[0])
		#movingAtomYCoord = 1.*float(el[1])*float(unitVector[1])
		movingAtomYCoord = (17./27.)*scaleUp * yStretch
		movingAtomZCoord = scaleUp*(float(el[1]))*zStretch/float(scalarFieldDimensions[2])	
		movingAtomECoord = float(el[2]) # is energy of h atom and thus not stretched
		
		movingAtomNewCoord = [movingAtomXCoord,movingAtomYCoord,movingAtomZCoord,movingAtomECoord]			
		movingAtomNewCoordsList.append(movingAtomNewCoord)
	
	''' 
	for i in range(bewegungsDauer-1):
		for j in range(interpolationsFaktor):
			interpolatedMovingAtomXCoord = ( movingAtomNewCoordsList[i+1][0]-movingAtomNewCoordsList[i][0] )*j/float(interpolationsFaktor)
			+ movingAtomNewCoordsList[i][0]
			interpolatedMovingAtomYCoord = ( movingAtomNewCoordsList[i+1][1]-movingAtomNewCoordsList[i][1] )*j/float(interpolationsFaktor)
			+ movingAtomNewCoordsList[i][1]
			interpolatedMovingAtomZCoord = ( movingAtomNewCoordsList[i+1][2]-movingAtomNewCoordsList[i][2] )*j/float(interpolationsFaktor)
			+ movingAtomNewCoordsList[i][2]
			
			interpolatedMovingAtomCoord = [interpolatedMovingAtomXCoord,interpolatedMovingAtomYCoord,interpolatedMovingAtomZCoord,movingAtomECoord]
			interpolatedMovingAtomCoordsList.append(interpolatedMovingAtomCoord)
		
	'''
	"""Glut init function."""
	glClearColor ( 0, 0, 0, 0 )
	
	glEnable( GL_DEPTH_TEST )
	glShadeModel( GL_SMOOTH )
	glEnable( GL_LIGHTING )
	### activates the lightning for the window
	glEnable( GL_LIGHT0 )
	glLightModeli( GL_LIGHT_MODEL_TWO_SIDE, 0 )
	glLightfv( GL_LIGHT0, GL_POSITION, [4, 4, 4, 1] )
	lA = 0.8
	glLightfv( GL_LIGHT0, GL_AMBIENT, [lA, lA, lA, 1] )
	lD = 1
	glLightfv( GL_LIGHT0, GL_DIFFUSE, [lD, lD, lD, 1] )
	lS = 1
	glLightfv( GL_LIGHT0, GL_SPECULAR, [lS, lS, lS, 1] )
	glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.2, 0.2, 0.2, 1] )
	glMaterialfv( GL_FRONT_AND_BACK, GL_DIFFUSE, [0.7, 0.7, 0.7, 1] )
	glMaterialfv( GL_FRONT_AND_BACK, GL_SPECULAR, [0.5, 0.5, 0.5, 1] )
	glMaterialf( GL_FRONT_AND_BACK, GL_SHININESS, 50 )
	
	teapotList = glGenLists( 1 )
	glNewList( teapotList, GL_COMPILE )
	
	print zShift , " = zShift "
	
	glMaterialfv( GL_FRONT_AND_BACK, GL_AMBIENT, [0.0, 0., 0.2, 0.1] )
	for i in newCoordsList:
		glTranslate(i[0], i[1], i[2])
		
		glutSolidSphere(0.1,16,16)
		glTranslate(-i[0], -i[1], -i[2])
		
		glTranslate(i[0], i[1]+yShift, i[2])
		
		glutSolidSphere(0.1,16,16)
		glTranslate(-i[0], -(i[1]+yShift), -i[2])
		
		
		
		glTranslate(i[0], i[1], i[2]+zShift)
		
		glutSolidSphere(0.1,16,16)
		glTranslate(-i[0], -i[1], -(i[2]+zShift))
		
		glTranslate(i[0], i[1]+yShift, i[2]+zShift)
		
		glutSolidSphere(0.1,16,16)
		glTranslate(-i[0], -(i[1]+yShift), -(i[2]+zShift))
	glEndList( )
	
	scalarFieldA = array(scalarField)#.reshape((280,26,32))
	
	maxFieldValue = max(scalarFieldA[0:-1,3])
	minFieldValue = min(scalarFieldA[0:-1,3])
	maxfV = maxFieldValue
	minfV = minFieldValue	
		
	fieldDataList = glGenLists(1)
	glNewList(fieldDataList,GL_COMPILE)
	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [0.0, 0., 0.2, 0.1])
	
		
	for xIndex in range(int(scalarFieldDimensions[0])):
		for zIndex in range(int(scalarFieldDimensions[2])):
			ind = (zIndex*( int(scalarFieldDimensions[0])*int(scalarFieldDimensions[1]) ) + (16*(int(scalarFieldDimensions[0])) + xIndex))
			L = 0.0195
			rVal = float(scalarFieldA[ind][3]) 			
			glTranslate( xIndex*scaleUp/float(scalarFieldDimensions[0]) , (17./27.)*scaleUp * yStretch , zIndex*scaleUp*zStretch/float(scalarFieldDimensions[2])	)
			#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rVal ,  1.*(0.5-rVal)*(0.5-rVal), (1-rVal)*(1-rVal) ,0.4]  )
			glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rVal*rVal ,  1.*(0.5-rVal)*(0.5-rVal), (1-rVal)*(1-rVal) ,0.4]  )
			#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [0.1 + ( 0.9*(float(scalarFieldA[ind][3])/maxfV) ), 0.25 - ( 0.25*(float(scalarFieldA[ind][3])/maxfV)*(float(scalarFieldA[ind][3])/maxfV)*(float(scalarFieldA[ind][3])/maxfV) ),0. ,0.4]  )
			glBegin(GL_POLYGON)
			glVertex3f(-L, 0.00, L)
			glVertex3f(L, 0.00, L)
			glVertex3f(L, 0.00, -L)
			glVertex3f(-L, 0.00, -L)
			glEnd()
			
			glTranslate( -xIndex*scaleUp/float(scalarFieldDimensions[0]), -(17./27.)*scaleUp * yStretch , -zIndex*scaleUp*zStretch/float(scalarFieldDimensions[2])	)
			
			
			
			
			glTranslate( xIndex*scaleUp/float(scalarFieldDimensions[0]) , (17./27.)*scaleUp * yStretch , zShift + zIndex*scaleUp*zStretch/float(scalarFieldDimensions[2])	)
			#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rVal, 1.*(0.5-rVal)*(0.5-rVal), (1-rVal)*(1-rVal) ,0.4]  )
			glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [rVal*rVal ,  1.*(0.5-rVal)*(0.5-rVal), (1-rVal)*(1-rVal) ,0.4]  )
			#glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, [0.1 + ( 0.9*(float(scalarFieldA[ind][3])/maxfV) ), 0.25 - ( 0.25*(float(scalarFieldA[ind][3])/maxfV)*(float(scalarFieldA[ind][3])/maxfV)*(float(scalarFieldA[ind][3])/maxfV) ),0. ,0.4]  )
			glBegin(GL_POLYGON)
			glVertex3f(-L, 0.00, L)
			glVertex3f(L, 0.00, L)
			glVertex3f(L, 0.00, -L)
			glVertex3f(-L, 0.00, -L)
			glEnd()
			
			glTranslate( -xIndex*scaleUp/float(scalarFieldDimensions[0]), -(17./27.)*scaleUp * yStretch , -(zShift + zIndex*scaleUp*zStretch/float(scalarFieldDimensions[2]) )	)

	glEndList( )
	
	now         = datetime.datetime.now()
	micro       = now.microsecond
	hour        = now.hour
	minute      = now.minute
	second      = now.second
	
	time_offset = hour*3600+minute*60+second+micro/1000000.0
	
	UpdateCamera()
	
def reshape( width, height ):
	"""Glut reshape function."""
	sC.whRatio = float(width)/float(height)
	sC.update( )
	### is called when window is resized or move

def keyPressed(*args):
### defines what happens when a key giving a ASCII char is pressed
	global xcam
	global ycam
	global zcam
	global angleX
	global angleY
	global angleZ
	global basis
	
	global freeze
	global index
	global index_freeze
	global mod_time
	global release_time
	global radius
	global bewegungsDauer
	global showEnergy
	
	
	# If escape is pressed, kill everything.
	if args[0] == ESCAPE:
		sys.exit()
	elif args[0] == 'r':
		xcam = 0.0
		ycam = 0.0
		zcam = 0.0
		angleX = 0.0
		angleY = 0.0
		angleZ = 0.0

	elif args[0] == 'j':
		angleZ += 1.0
	elif args[0] == 'l':
		angleZ -= 1.0

	elif args[0] == 'u':
		angleY += 1.0
	elif args[0] == 'o':
		angleY -= 1.0

	elif args[0] == 'i':
		angleX += 1.0
	elif args[0] == 'm':
		angleX -= 1.0

	elif args[0] == 'k':
		angleX = 0.0
		angleZ = 0.0
	elif args[0] == 'b':
		basis *= 1.05
		UpdateCamera()
	elif args[0] == 'B':
		basis /= 1.05
		UpdateCamera()
		
	elif args[0] == ' ':
		freeze = 1-freeze
		if (freeze == 1):
			index_freeze = index
		release_time = mod_time
	elif args[0] == '+'  or args[0] == ']':
	  if (freeze == 1):
			index_freeze = (index_freeze + 1) % bewegungsDauer
	elif args[0] == '-' or args[0] == '=':
	  if (freeze == 1):
			index_freeze = (index_freeze - 1) % bewegungsDauer	

	elif args[0] == 'S':
		radius *= 1.1
	elif args[0] == 's':
		radius /= 1.1
	elif args[0] == 'e':
		showEnergy = 0
	elif args[0] == 'E':
		showEnergy = 1

def MouseWithoutKey(*args):
	pass
	#print "Mouse is moving", args[0], args[1]
	#worldCoordinate = gluUnProject(x,y,z, self.modelViewMatrix, self.projectionMatrix, viewport)

def specialKey(*args):
	global xcam
	global ycam
	global zcam
	# print "Special key pressed"
	if args[0] == GLUT_KEY_LEFT:
		xcam -= 0.05
	elif args[0] == GLUT_KEY_RIGHT:
		xcam += 0.05
	elif args[0] == GLUT_KEY_UP:
		ycam += 0.05
	elif args[0] == GLUT_KEY_DOWN:
		ycam -= 0.05
	elif args[0] == GLUT_KEY_PAGE_UP:
		zcam += 0.05
	elif args[0] == GLUT_KEY_PAGE_DOWN:
		zcam -= 0.05
		

######## main program ##########

teapotList = 0
xcam = -2.1
ycam = -0.65
zcam = -0.0
angleX = 90. # -66.0 # 0.0
angleY = 0.0
angleZ = 0.0

basis = 200.0
radius = 0.1
freeze = 1
index_freeze = 1
release_time = 0
mod_time = 0

showEnergy = 0

if len( argv ) != 2:
	print "Usage:"
	print "python stereDemo.py SHUTTER | ANAGLYPH | NONE \n"
else:
	glutInit( sys.argv )	###glutInit() initializes the glut library & processes command line options, 
							###must be called first
	stereoMode = sys.argv[1].upper( )
	if stereoMode == "SHUTTER":
		glutInitDisplayMode( GLUT_STEREO | GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH )
		### specifies the display mode when the glutCreateWindow() is called
		### default (GLUT_RGBA | GLUT_SINGLE ) sets an RGBA single buffered window when create is called
	else:
		glutInitDisplayMode( GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH )
            
	glutInitWindowSize( 500, 500 )
	###sets size and position for windows created by call of glutCreateWindow()
	glutInitWindowPosition( 100, 100 )
	glutCreateWindow( sys.argv[0] )
	### creates a window with the previously set characteristics, which will be displayed as soon as
	### (see below) glutMainLoop() is started
	init(  )
	
	glutFullScreen( )

	glutDisplayFunc( display )
	### sets which function is called when contents of the window have to be redrawn, see 
	### def display() above; argument is a unrestricted (void) function pointer
	glutReshapeFunc( reshape )
	### sets function which has to be called when window is resized or moved, see def reshape() above
	### reshape() gets two arguments, int width, int height
	glutKeyboardFunc( keyPressed )
	### sets the function which is called when a key giving an ASCII char is pressed, see 
	### keyPressed() above, arguments are int key,int x, int y which give the pressed key and 
	### the x and y position of the mouse when the key was pressed
	glutSpecialFunc( specialKey)
	
	glutPassiveMotionFunc(MouseWithoutKey)
	
	glutIdleFunc( animationStep )
	### specifies a function which will be executed if no other events are pending 
	### passing ZERO disables called function
	
	glutMainLoop(  )
	### enters GLUT process loop: registered callback functions will be called when the defined events
	### for which they should start occur
