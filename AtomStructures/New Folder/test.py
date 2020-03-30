import numpy as np
for i in np.linspace(0,16,17):
    file = open('MD_hydrogen.%i'%i,'r')
    lst = []
    for line in file:
        lst += [line.split()]
    
    f2=open('test_%i.txt'%i,'w')
    f2.write('anim 64 0 %i 16 "Fe-Structure"\n'%i)
    for i in range(len(lst)):
        a1=float(lst[i][0])
	a2=float(lst[i][1])
	a3=float(lst[i][2])
	a4='Fe'
	
	f2.write(str(a1))
	f2.write(' ')
	f2.write(str(a2))
	f2.write(' ')
	f2.write(str(a3))
	f2.write(' ')
	f2.write(str(a4))
	f2.write('\n')
    f2.write('empty')
    f2.close()
