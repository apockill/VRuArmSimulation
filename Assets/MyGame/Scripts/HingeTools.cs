using UnityEngine;
using System.Collections;

public static class  HingeTools{

    public static void setAngle(HingeJoint hingeJoint, float angle)
    {
        JointLimits jLimit = new JointLimits();
        jLimit.min = angle;
        jLimit.max = angle;
        hingeJoint.limits = jLimit;

    }

}
